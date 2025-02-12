import React, { createContext, useContext, useState, ReactNode,useEffect } from 'react';
import { AvatarGenderTypeEnum } from '../api/GeneratedApiClient';

// Define the structure of the shared state
interface AppState {
    user: string | null;
    gender: AvatarGenderTypeEnum | AvatarGenderTypeEnum.Male;
    setUser: (user: string | null) => void;
    setGender: (gender: AvatarGenderTypeEnum) => void;
    userType: number | null;
    setUserType: (userType: number | null) => void;
}
interface AppProviderProps {
    children: ReactNode;
}

// Create the context with a default value
const AppContext = createContext<AppState | undefined>(undefined);

// Provider Component
export const AppProvider: React.FC<AppProviderProps> = ({ children }) => {
    const [user, setUser] = useState<string | null>(()=>localStorage.getItem("user"));
    const [gender, setGender] = useState<AvatarGenderTypeEnum | null>(() => {
        var gender = localStorage.getItem("gender");
        return gender != null? gender as AvatarGenderTypeEnum : AvatarGenderTypeEnum.Male;
    });
    const [userType, setUserType] = useState<number | null>(()=>Number(localStorage.getItem("userType")) || 0);

    useEffect(() => {
        if (user !== null)
            localStorage.setItem("user", user);
    }, [user]);

    useEffect(() => {
        localStorage.setItem("gender", gender.toString()); // Store as string        
    }, [gender]);

    useEffect(() => {
        localStorage.setItem("userType", userType?.toString() ?? "0");
    }, [userType]);

    return (
        <AppContext.Provider value={{ user, gender, setUser, setGender, userType, setUserType}}>
            {children}
        </AppContext.Provider>
    );
};

// Custom Hook for consuming the context
export const useAppContext = () => {
    const context = useContext(AppContext);
    if (!context) {
        throw new Error('useAppContext must be used within an AppProvider');
    }
    return context;
};
