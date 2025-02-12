import React from 'react';
import { Accordion, AccordionSummary, AccordionDetails, Typography, List, ListItem, ListItemText, Box } from '@mui/material';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import { Unit, PossibleAnswer, Prompt, RequestedInformation } from '../api/GeneratedApiClient';

const ChatUnits = ({ units }: { units: Unit[] }) => {
    return (
        <Box sx={{ p: 2, maxHeight: '80vh', overflowY: 'auto' }}>
            {units?.map((unit:Unit) => (
                <Accordion key={unit.id}>
                    {/* Unit Details */}
                    <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                        <Typography variant="h6">{unit.name}</Typography>
                        <Typography variant="subtitle2" sx={{ ml: 2, color: 'gray' }}>
                            {`(${unit.type})`}
                        </Typography>
                    </AccordionSummary>
                    <AccordionDetails>
                        {/* Prompts for Each Unit */}
                        {unit.prompts?.map((prompt:Prompt) => (
                            <Box key={prompt.id} sx={{ mb: 2, p: 2, border: '1px solid #ccc', borderRadius: 2 }}>
                                <Typography variant="subtitle1">
                                    <strong>Goal:</strong> {prompt.goal}
                                </Typography>
                                <Typography variant="body1" sx={{ mt: 1 }}>
                                    <strong>Message:</strong> {prompt.message?.text}
                                </Typography>

                                {/* Possible Answers */}
                                <Typography variant="body2" sx={{ mt: 2, fontWeight: 'bold' }}>
                                    Possible Answers:
                                </Typography>
                                <List>
                                    {(prompt.possibleAnswers || []).length > 0 ? (
                                        prompt.possibleAnswers?.map((answer: PossibleAnswer) => (
                                            <ListItem key={answer.promptId} sx={{ textAlign: 'center' }}>
                                                <ListItemText primary={answer.userPrompt} />
                                            </ListItem>
                                        ))
                                    ) : (
                                        <Typography sx={{ ml: 2 }}>No answers available</Typography>
                                    )}
                                </List>

                                {/* Requested Information */}
                                <Typography variant="body2" sx={{ mt: 2, fontWeight: 'bold' }}>
                                    Requested Information:
                                </Typography>
                                <List>
                                    {prompt.requestedInformation?.map((info: RequestedInformation, index:number) => (
                                        <ListItem key={index} sx={{ textAlign: 'center' }}>
                                            <ListItemText
                                                primary={`${info.name} (${info.type})`}
                                                secondary={info.description}
                                            />
                                        </ListItem>
                                    ))}
                                </List>
                            </Box>
                        ))}
                    </AccordionDetails>
                </Accordion>
            ))}
        </Box>
    );
};

export default ChatUnits;
