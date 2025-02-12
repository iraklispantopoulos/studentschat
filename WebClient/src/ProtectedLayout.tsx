import React from 'react';
import { Outlet, Navigate, useNavigate } from 'react-router-dom';
import { AppBar, Toolbar, Drawer, List, ListItemButton, ListItemText, Typography, Box,Button } from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import IconButton from '@mui/material/IconButton';
import ExitToAppIcon from '@mui/icons-material/ExitToApp';
import SettingsIcon from "@mui/icons-material/Settings";
import { GetToken, RemoveToken } from './services/AuthHelper';
import { useAppContext } from './context/AppContext';

const ProtectedLayout = () => {
    const { userType } = useAppContext();
    const token = GetToken();
    const navigate = useNavigate();
    const gotoChat = () => {
        navigate('/chat');
    };
    const menuItemsForTeacher = [
        { text: 'Ιστορικό', route: '/chat-history'},
        { text: 'ρυθμίσεις', route: '/configuration'}        
    ];
    const menuItemsForStudent = [
        { text: 'Νέα συνομιλία', route: `/chat`}
    ];
    if (!token) {
        return <Navigate to="/" replace />;
    }
    const handleLogout = () => {
        RemoveToken();
        navigate('/');
    };

    return (
        <Box sx={{ display: 'flex', height: '100vh' }}>
            {/* AppBar for Header */}
            <AppBar
                position="fixed"
                sx={{
                    zIndex: (theme) => theme.zIndex.drawer + 1,
                }}
            >
                <Toolbar>
                    <IconButton color="inherit" aria-label="menu" edge="start" sx={{ mr: 2 }}>
                        <MenuIcon />
                    </IconButton>
                    <Typography
                        variant="h6"
                        noWrap
                        sx={{
                            cursor: 'pointer', // Hand icon
                            '&:hover': {
                                color: 'secondary.main', // Optional hover effect for color
                            },
                        }}
                        onClick={gotoChat}
                    >
                        Έξυπνος βοηθός για μαθητές
                    </Typography>   
                    <Box sx={{ marginLeft: "auto", display: "flex", gap: 1 }}>
                        <Button
                            variant="contained"
                            color="secondary"
                            startIcon={<SettingsIcon />}
                            onClick={() => navigate('/settings')}
                            sx={{ marginLeft: 'auto', textTransform: "none" }} // This moves the button to the right
                        >
                            Ρυθμίσεις
                        </Button>
                        <Button
                            variant="contained"
                            color="primary"
                            startIcon={<ExitToAppIcon />}
                            onClick={ handleLogout }
                            sx={{ marginLeft: 'auto', textTransform: "none" }} // This moves the button to the right
                        >
                            Απόσύνδεση
                        </Button>
                    </Box>                    
                </Toolbar>
            </AppBar>

            {/* Drawer for Side Menu */}
            <Drawer
                variant="permanent"
                sx={{
                    width: 130,
                    flexShrink: 0,
                    [`& .MuiDrawer-paper`]: {
                        width: 130,
                        boxSizing: 'border-box',
                    },
                }}
            >
                <Toolbar />
                <List>
                    {userType == 1 && menuItemsForTeacher.map((item) => (
                        <ListItemButton key={item.text} onClick={() => navigate(item.route)}>
                            <ListItemText primary={item.text} />
                        </ListItemButton>
                    ))}
                    {userType == 0 && menuItemsForStudent.map((item) => (
                        <ListItemButton key={item.text} onClick={() => navigate(item.route, { state: { chatMode: "new_chat"}})}
>
                            <ListItemText primary={item.text} />
                        </ListItemButton>
                    ))}
                </List>
            </Drawer>

            {/* Main Content Area */}
            <Box
                component="main"
                sx={{
                    flexGrow: 1,
                    p: 0,
                    mt: 8, // Offset to avoid overlapping AppBar
                    display: 'flex',
                    alignItems: 'center', // Vertically center the Outlet
                    justifyContent: 'center', // Horizontally center the Outlet
                }}
            >
                <Outlet />
            </Box>
        </Box>
    );
};

export default ProtectedLayout;
