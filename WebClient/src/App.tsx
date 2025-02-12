import React from 'react';
import AuthForm from './components/forms/AuthForm';
import Chat from './pages/Chat';
import ChatHistory from './pages/ChatHistory';
import ProtectedLayout from './ProtectedLayout';
import Configuration from './pages/Configuration';
import { BrowserRouter as Router, Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import { AppProvider } from './context/AppContext';
import { GetToken } from './services/AuthHelper';
import { ApiClient } from './api/GeneratedApiClient';
import Settings from './pages/SettingsPage';
import './App.css'

function App() {
	const originalFetch = window.fetch;
	window.fetch = async (url, options) => {
		try {
			if ((url as string).indexOf("api") >= 0) {
				const token = GetToken();
				options = {
					...options,
					headers: {
						...options?.headers,
						Authorization: token ? `bearer ${token}` : "",
					},
				};
			}
		} catch (err) {
			console.log(err);
		}
		return originalFetch(url, options);
	};
	setInterval(() => {
		if (!(window.location.pathname == '/')) {
			new ApiClient(import.meta.env.VITE_API_URL)
				.trylogin()
				.catch((error) => {
					if (error.status == '401') {
						window.location.href = '/';
					}
				});
		}
	}, 5000);
	return (
		<>
			<AppProvider>
				<Router>
					<Routes>
						<Route path="/" element={<AuthForm />} />
						<Route element={<ProtectedLayout />}>
							<Route path="/chat" element={<Chat />} />
							<Route path="/chat-history" element={<ChatHistory />} />
							<Route path="/configuration" element={<Configuration />} />
							<Route path="/settings" element={<Settings />} />
						</Route>
					</Routes>
				</Router>
			</AppProvider>
		</>
	)
}

export default App
