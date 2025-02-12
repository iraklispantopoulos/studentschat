export const SaveToken = (token: string) => {
	localStorage.setItem('authToken', token);
};

export const GetToken = (): string | null => {
	return localStorage.getItem('authToken');
};

export const RemoveToken = () => {
	localStorage.removeItem('authToken');
};
