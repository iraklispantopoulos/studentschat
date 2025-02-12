import React from 'react';
import { Button, Box } from '@mui/material';
import MaleIcon from '@mui/icons-material/Male';
import FemaleIcon from '@mui/icons-material/Female';
import { SendToApi } from '../services/HttpClient';
import { useAppContext } from '../context/AppContext';
import { ApiClient, AvatarGenderTypeEnum } from '../api/GeneratedApiClient';

const GenderButtons: React.FC = () => {
    const { gender, setGender } = useAppContext();
    const saveGender = async (gender: AvatarGenderTypeEnum) => {
        new ApiClient(import.meta.env.VITE_API_URL)
            .setAvatarGender({
                type:gender
            })
            .then(() => {
                setGender(gender);
            })
			.catch((error) => {
				console.error(error);
			});
        //await SendToApi({ Type: gender }, `${import.meta.env.VITE_API_URL}/User/SetAvatarGender`);        
    };    
    return (
        <Box display="flex" gap={2} justifyContent="center" mt={2}>
            <Button onClick={()=>saveGender(AvatarGenderTypeEnum.Male)}
                variant="contained"
                color="primary"
                startIcon={<MaleIcon />}
            >
                Άνδρας
            </Button>
            <Button onClick={() => saveGender(AvatarGenderTypeEnum.Female)}
                variant="contained"
                color="secondary"
                startIcon={<FemaleIcon />}
            >
                Γυναίκα
            </Button>
        </Box>
    );
};

export default GenderButtons;
