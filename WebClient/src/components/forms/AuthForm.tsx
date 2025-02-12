import React, { useState, ChangeEvent, FormEvent } from 'react';
import { useNavigate } from 'react-router-dom';
import { TextField, Button, Box, Typography, Link } from '@mui/material';
import { SendToApi } from '../../services/HttpClient';
import { SaveToken } from '../../services/AuthHelper';
import { useAppContext } from '../../context/AppContext';

interface AuthData {
    password: string;
    username?: string; // Only for signup
}

const AuthForm: React.FC = () => {
    const { setGender, setUserType } = useAppContext();
    const navigate = useNavigate();
    const [isSignup, setIsSignup] = useState(false);
    const [wrongCredentials, setWrongCredentials] = useState(false);
    const [formData, setFormData] = useState<AuthData>({
        password: '',
        username: '',
    });

    const handleChange = (e: ChangeEvent<HTMLInputElement>) => {
        setFormData({ ...formData, [e.target.name]: e.target.value });
    };

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        const apiUrl = isSignup
            ? `${import.meta.env.VITE_API_URL}/api/auth/signup`
            : `${import.meta.env.VITE_API_URL}/api/auth/login`;

        try {
            const payload = isSignup
                ? formData
                : { username: formData.username, password: formData.password };
            const result = await SendToApi(payload, apiUrl);
            console.log(`${isSignup ? 'Signup' : 'Login'} successful:`, result);
            if (result.token) {
                SaveToken(result.token); 
                setGender(result.gender);
                setUserType(result.type=='Teacher'?1:0);
                console.log('Token stored successfully!');
                navigate('/chat');
            }
        } catch (error) {
            console.error(`${isSignup ? 'Signup' : 'Login'} error:`, error);			
            if ((error as Error).message.indexOf('401') >= 0)
				setWrongCredentials(true);
        }
    };

    return (
        <Box sx={{ width: 300, mx: 'auto', mt: 5 }}>
            <Typography variant="h5" gutterBottom>
                {isSignup ? 'Εγγραφή' : 'Σύνδεση'}
            </Typography>
            <form onSubmit={handleSubmit}>
                <TextField
                    label="Όνομα"
                    name="username"
                    value={formData.username}
                    onChange={handleChange}
                    fullWidth
                    margin="normal"
                    required
                />               
                <TextField
                    label="Κωδικός"
                    name="password"
                    type="password"
                    value={formData.password}
                    onChange={handleChange}
                    fullWidth
                    margin="normal"
                    required
                />
                <Button type="submit" variant="contained" fullWidth sx={{ mt: 2 }}>
                    {isSignup ? 'Εγγραφή' : 'Σύνδεση'}
                </Button>
            </form>
            <Typography variant="body2" align="center" sx={{ mt: 2 }}>
                {isSignup ? 'Έχεις κάνει εγγραφή?' : "Δεν έχεις κάνει εγγραφή?"}{' '}
                <Link
                    component="button"
                    variant="body2"
                    onClick={() => setIsSignup(!isSignup)}
                >
                    {isSignup ? 'Σύνδεση' : 'Εγγραφή'}
                </Link>
                {wrongCredentials && (
                    <Typography variant="body2" color="error">
                        Δεν βρέθηκε χρήστης με αυτά τα στοιχεία. Προσπαθήστε ξανά!
                    </Typography>
                )}
            </Typography>
        </Box>
    );
};

export default AuthForm;
