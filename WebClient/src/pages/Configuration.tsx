import { useState, useEffect } from 'react'
import { Unit, ApiClient } from '../api/GeneratedApiClient';
import ChatUnits from '../components/ChatUnits';

const Configuration = () => {    
	const [units, setUnits] = useState<Unit[] | null>(null);
	useEffect(() => {
		new ApiClient(import.meta.env.VITE_API_URL)
			.getUnits()
			.then((units) => {
				setUnits(units);
			})
			.catch((error) => {
				console.error(error);
			});		
	}, []);	
    return (
		<ChatUnits units={units || []}/>
    );
};

export default Configuration;
