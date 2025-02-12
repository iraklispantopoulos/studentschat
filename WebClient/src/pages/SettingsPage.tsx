import React from 'react';
import { useNavigate } from "react-router-dom";
import ArrowBackIcon from "@mui/icons-material/ArrowBack";
import { Box, Button } from '@mui/material';
import Gender from '../components/Gender';

const Settings = () => {
	const navigate = useNavigate();
	return (
		<>
            <Box
                sx={{
                    display: "flex",
                    flexDirection: "column",
                    height: "100vh", // Full viewport height
                }}
            >
                {/* Back Button in the Top-Left Corner */}
                <Box
                    sx={{
                        position: "fixed", // Keeps button in top-left regardless of scrolling
                        top: 80,
                        left: 150,
                        zIndex: 1000, // Ensures it stays above other content
                    }}
                >
                    <Button
                        variant="contained"
                        color="primary"
                        startIcon={<ArrowBackIcon />}
                        onClick={() => navigate(-1)}
                        sx={{ textTransform: "none" }}
                    >
                        Επιστροφή
                    </Button>
                </Box>

                {/* Centered Gender Component */}
                <Box
                    sx={{
                        display: "flex",
                        flexGrow: 1, // Fills remaining space
                        justifyContent: "center", // Centers horizontally
                        alignItems: "center", // Centers vertically
                    }}
                >
                    <Gender />
                </Box>
            </Box>
		</>
	);
};

export default Settings;
