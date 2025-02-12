import React, { useEffect, useState } from 'react';
import { List, ListItem, ListItemAvatar, ListItemText, Avatar, Typography } from '@mui/material';
import { ApiClient, User } from '../api/GeneratedApiClient';

const UserList = ({ onUserSelect }: { onUserSelect:(user: User) => void}) => {
	const [users, setUsers] = useState<User[] | null>(null);
	const [selectedIndex, setSelectedIndex] = useState<number | null>(null);	
	const handleItemClick = (user: User,index:number) => {
		setSelectedIndex(index);
		onUserSelect(user);
	};
	useEffect(() => {
		new ApiClient(import.meta.env.VITE_API_URL)
			.getAllStudents()
			.then((students) => {
				setUsers(students);
			})
			.catch((error) => {
				console.error(error);
			});
	}, []);

	return (
		<div style={{
			height: '45em',
			overflow: 'auto'
		}}>
			<Typography variant="h5" gutterBottom>
				User List
			</Typography>
			<List>
				{users?.map((user: User, index: number) => (
					<ListItem	
						style={{ background: selectedIndex === index ?'gainsboro':'inherit'}}
						component={"div" as any}
						key={index}
						button
						onClick={() => handleItemClick(user,index)}
						sx={{
							cursor: 'pointer',
						}}
					>
						<ListItemAvatar>
							<Avatar>{user.name?.charAt(0)}</Avatar>
						</ListItemAvatar>
						<ListItemText primary={user.name} secondary={user.avatarGenderType} />
					</ListItem>
				))}
			</List>
		</div >
	);
};

export default UserList;
