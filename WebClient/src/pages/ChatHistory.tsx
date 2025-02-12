import React from 'react';
import { useState,useEffect } from 'react'
import { Box, List, ListItem, ListItemText, Typography, Button, Stack, TextField } from '@mui/material';
import { ChatSession, ChatRecord, PromptTypeEnum, ApiClient, User } from '../api/GeneratedApiClient';
import UsersList from '../components/UsersList';

const ChatHistory = () => {
	const [loading, setLoading] = useState(false);
	const [userChatPage, setUserChatPage] = useState<number>(1);
	const [selectedDate, setSelectedDate] = useState<Date | null>(null);
	const [chatSession, setChatSession] = useState<ChatSession | null>(null);
	const [selectedUser, setSelectedUser] = useState<User | null>(null);
	const onUserSelect = (user: User) => {
		if (selectedUser?.id != user.id) {
			setUserChatPage(1);
			setChatSession(null);
			setSelectedUser(user);			
		}
	};
	useEffect(() => {
		getUserChatHistory();
	}, [selectedUser])
	useEffect(() => {
		setUserChatPage(1);
		setChatSession(null);
		getUserChatHistory();
	}, [selectedDate])
	const onDateChange = (event: React.ChangeEvent<HTMLInputElement>) => {
		setSelectedDate(new Date(event.target.value));		
	};
	const onUsersScroll = (event: React.UIEvent<HTMLUListElement, UIEvent>) => {
		const bottom = event.currentTarget.scrollHeight === event.currentTarget.scrollTop + event.currentTarget.clientHeight;
		if (bottom && !loading) {
			getUserChatHistory();
		}
	};
	const getUserChatHistory = () => {
		setLoading(true);
		if (selectedUser != null)
			new ApiClient(import.meta.env.VITE_API_URL)
				.getChat(selectedUser?.id, selectedDate??new Date(), userChatPage)
				.then((_chatSession) => {
					setLoading(false);
					setUserChatPage(p => p + 1);
					setChatSession((p) => {
						if (!p)
							return _chatSession;

						// Create a copy of the existing records and add new records
						const updatedRecords = [...(p.records || []), ...(_chatSession?.records || [])];

						// Return the updated state
						return { ...p, records: updatedRecords };
					});
				})
				.catch((error) => {
					console.error(error);
				});
	};
	return (
		<>
			<div
				style={{
					width: '100%',
					height: '90%'
				}}
			>
				<Box sx={{ display: 'flex', gap: 2 }}>
					<Box sx={{ p: 2, borderRadius: 1, width: 200 }}>
						<div>
							<TextField
								label="επιλογή ημερομηνίας"
								type="date"
								onChange={onDateChange}
								InputLabelProps={{
									shrink: true, // Ensures the label stays visible
								}}
								fullWidth
							/>
						</div>
						<UsersList onUserSelect={onUserSelect} />
					</Box>
					<Box
						sx={{
							maxHeight: '800px',
							width: '800px',
							overflowY: 'auto',
							p: 2,
							border: '1px solid #ccc',
							borderRadius: 2,
							flex: 1
						}}
						component={"div" as any}
						onScroll={onUsersScroll}>
						<Typography variant="h6" gutterBottom>
							Ιστορικό συνομιλίας
							<Typography
								component="span"
								variant="h6"
								sx={{ fontWeight: 'bold', color: 'black', marginLeft: '8px',opacity:0.5 }}
							>
								({selectedUser?.name})
							</Typography>
						</Typography>
						{
							chatSession && chatSession.records?.length == 0 &&
								(
									<div>δεν υπάρχει ιστορικό..</div>
								)
						}
						<List onScroll={onUsersScroll}>
							{chatSession?.records?.map((chatRecord: ChatRecord, index: number) => (
								<ListItem
									component={"div" as any}
									key={index}
									sx={{
										justifyContent: chatRecord.promptType === PromptTypeEnum.User ? 'flex-start' : 'flex-end',
										display: 'flex',
									}}
								>
									<Box
										sx={{
											maxWidth: '70%',
											p: 1,
											borderRadius: 2,
											bgcolor: chatRecord.promptType === PromptTypeEnum.User ? 'primary.light' : 'secondary.light',
											color: 'black',
										}}
									>
										<Typography variant="body2" gutterBottom>
											{chatRecord.message}
										</Typography>
										<Typography variant="caption" sx={{ display: 'block', textAlign: 'right' }}>
											{(new Date(chatRecord?.date || '')).toLocaleTimeString('el-GR', { timeZone: 'Europe/Athens' })}
										</Typography>
									</Box>
								</ListItem>
							))}
						</List>
					</Box>
				</Box>
			</div>
		</>
	);
};

export default ChatHistory;
