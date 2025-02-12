import { useState, useRef, useEffect, Ref } from 'react'
import { useLocation } from "react-router-dom";
import { CircularProgress,Divider } from "@mui/material";
import { Canvas, useThree } from "@react-three/fiber";
import { Environment, useTexture } from "@react-three/drei";
import Avatar, {AvatarRef} from '../components/Avatar';
import { SendToApi, PollUrl } from '../services/HttpClient';
import { Speech } from '../models/Shared';
import { useAppContext } from '../context/AppContext';
import { ApiClient, AvatarGenderTypeEnum,AgentResponse } from '../api/GeneratedApiClient';
enum InteractionState {
	ReadyToStartListening = 0,
	Listening = 1,
	WaitingForResponse = 2,
	ResponseHasBeenReceived = 3,
	ResponseHasBeenPlayed = 4
}
interface AvatarSceneProps {
	speech: Speech,
	gender: AvatarGenderTypeEnum,
	avatarRef:Ref<AvatarRef>
}
function AvatarScene({ speech, gender, avatarRef }: AvatarSceneProps) {
	const texture = useTexture('/textures/classroom.jpeg');
	const { viewport } = useThree();
	const aspect = texture.image.width / texture.image.height;
	const planeWidth = viewport.width;
	const planeHeight = planeWidth / aspect;
	return (
		<>
			<color attach="background" args={["#ececec"]} />
			<Avatar ref={avatarRef} key={gender} groupProps={{ position: [0, -3, 5], scale: 2 }} speech={speech} gender={gender} />
			<Environment preset="sunset" />
			<mesh>
				<planeGeometry args={[planeWidth, planeHeight]} />
				<meshBasicMaterial map={texture} />
			</mesh>
		</>
	);
}
function Chat() {
	var webApiSpeechUrl = `${import.meta.env.VITE_API_URL}/api/TextToSpeech`;
	var speechResponsesAudioFilesUrl = import.meta.env.VITE_SPEECH_RESPONSES_URL;
	const location = useLocation();	
	const isEnabled = location.state?.chatMode ?? false; 
	const avatarRef = useRef<AvatarRef | null>(null);
	const { gender } = useAppContext();
	const getSpeechUrl = (fileName: string) => `${speechResponsesAudioFilesUrl}/${fileName}`;
	const getLipSyncUrl = (fileName: string) => `${speechResponsesAudioFilesUrl}/${fileName}`;
	const [lastAgentResponse, setLastAgentResponse] = useState<{ speechUrl: string, lipSyncUrl: string } | null>(null);
	const [showForm, setShowForm] = useState(false);
	const [loading, setLoading] = useState(true);
	const [agentResponse, setAgentResponse] = useState<AgentResponse | null>(null);
	const [transcript, setTranscript] = useState('');	
	const [speech, setSpeech] = useState<Speech | null>(null);
	const [interimTranscript, setInterimTranscript] = useState('');	
	const [interactionState, setInteractionState] = useState(InteractionState.ReadyToStartListening);
	let recognition: any;
	const interactionStateRef = useRef(interactionState);
	useEffect(() => {
		interactionStateRef.current = interactionState;
	}, [interactionState]);
	useEffect(() => {
		if (interactionStateRef.current == InteractionState.ResponseHasBeenReceived && agentResponse !== null) {
			const speechUrl = getSpeechUrl(agentResponse.speechFilename ?? "");
			const lipSyncUrl = getLipSyncUrl(agentResponse.lipSyncFilename ?? "");
			PollUrl(speechUrl)
				.then(async () => {
					console.log("speech received for " + agentResponse.speechFilename);
					return PollUrl(lipSyncUrl);
				})
				.then(() => {
					console.log("lipsync received for " + agentResponse.lipSyncFilename);
					setSpeech(new Speech(speechUrl, lipSyncUrl, agentResponse.speechFilename??""));
					setLastAgentResponse({ speechUrl, lipSyncUrl });
					setInteractionState(InteractionState.ReadyToStartListening);
				})
				.catch((error) => {
					setInteractionState(InteractionState.ReadyToStartListening);
					console.error("Polling error:", error);
				});
			return () => { }
		}

	}, [interactionState, agentResponse]);
	useEffect(() => {
		if (location.state) {
			switch (location.state?.chatMode) {
				case 'new_chat':
					startConversation(true);
					break;
				case 'continue_chat':
					startConversation(false);
					break;
				default:
					break;
			}
		} else
			setLoading(false);
	}, [location.state]);

	const audioRef = useRef<HTMLAudioElement | null>(null);
	const playAudio = (audioPath: string, onEndcallback: (() => void) | null = null) => {
		if (!audioRef.current) {
			audioRef.current = new Audio(audioPath);
		}
		audioRef.current.play();
		audioRef.current.onended = onEndcallback;
	};


	if ('webkitSpeechRecognition' in window || 'SpeechRecognition' in window) {
		const SpeechRecognition =
			(window as any).SpeechRecognition || (window as any).webkitSpeechRecognition;
		recognition = new SpeechRecognition();
		recognition.continuous = true;
		recognition.interimResults = true;
		recognition.lang = 'el-GR';

		recognition.onresult = (event: any) => {
			if (interactionStateRef.current == InteractionState.Listening) {
				var _interimTranscript = '';
				for (let i = event.resultIndex; i < event.results.length; i++) {
					const transcriptPart = event.results[i][0].transcript;
					_interimTranscript += transcriptPart;
					console.log(_interimTranscript);
					//if (event.results[i].isFinal) {
					//	setTranscript((prev) => prev + transcriptPart);
					//} else {						
					//	_interimTranscript += transcriptPart;						
					//}
				}
				setInterimTranscript(_interimTranscript);
			}
		};

		recognition.onerror = (event: any) => {
			console.error(`Error occurred: ${event.error}`);
		};
	} else {
		console.error('Speech Recognition API is not supported in this browser.');
	}
	const playIntroSound = () => {
		playAudio(`${speechResponsesAudioFilesUrl}/init-mic-white-noise.mp3`, () => {
			setInteractionState(InteractionState.ReadyToStartListening);
			setShowForm(true);
		});
	}
	const repeatQuestion = () => {
		if (avatarRef.current) {
			setInteractionState(InteractionState.ReadyToStartListening);
			avatarRef.current.repeatSpeech();
		}
	}
	const startConversation = (newConversation:boolean) => {
		setLoading(true);
		playIntroSound();
		setTranscript("");
		setInteractionState(InteractionState.WaitingForResponse);
		recognition.stop();
		new ApiClient(import.meta.env.VITE_API_URL)
			.start({
				newConversation: newConversation
			})
			.then((agentResponse) => {								
				setAgentResponse(agentResponse);
				setInteractionState(InteractionState.ResponseHasBeenReceived);
				setLoading(false);
				setSpeech(new Speech(
					getSpeechUrl(agentResponse.speechFilename ?? ""),
					getLipSyncUrl(agentResponse.lipSyncFilename ?? ""),
					agentResponse.speechFilename ?? ""
				));
				console.log('Response from API:', JSON.stringify(agentResponse));
			})
			.catch((error) => {
				console.error(error);
			});		
	}
	const startListening = () => {
		setTranscript("");
		setInteractionState(InteractionState.Listening);
		recognition.start();
	};

	const stopListening = () => {
		setTranscript(interimTranscript);
		setInteractionState(InteractionState.WaitingForResponse);
		recognition.stop();
		new ApiClient(import.meta.env.VITE_API_URL)
			.processResponse({
				text: interimTranscript
			})
			.then((agentResponse) => {
				setInteractionState(InteractionState.ResponseHasBeenReceived);
				setAgentResponse(agentResponse);
				console.log('Response from API:', JSON.stringify(agentResponse));
			})
			.catch((error) => {
				console.error(error);
			});		
	};
	return (
		<>
			<div>
				{!showForm && !loading &&
					<>
						<button onClick={() => startConversation(true)}>
							Εναρξη νέας συνομιλίας
						</button>
						<button onClick={() => startConversation(false)}>
							Συνέχιση συνομιλίας
						</button>						
					</>
				}				
				{loading &&
					<div>						
						<span>"Προετοιμασία..."</span>
						<CircularProgress color="inherit" />
					</div>					
				}
				{!loading &&
					<div>												
						<div style={{ display: showForm || loading ? 'block' : 'none' }}>
							<div style={{
								width: '75vw',
								height: '75vh',
								display: 'block',
								justifyContent: 'center',
								alignItems: 'center',
								margin: '0', // Remove any default margins
								overflow: 'hidden' // Prevent scrollbars
							}}>
								<div style={{
									width: '95%',
									height: '100%',
									border: '1px solid black',
									display: 'block', // Optional: Centers content inside this div if needed
									justifyContent: 'center',
									alignItems: 'center'
								}}>
									<Canvas shadows camera={{ position: [0, 0, 8], fov: 40 }}>
										<AvatarScene avatarRef={avatarRef} speech={speech!} gender={gender} />
									</Canvas>
								</div>
							</div>
							{/*{error && <p style={{ color: 'red' }}>{error}</p>}*/}
							<div>
								
								<button onClick={startListening} disabled={interactionState != InteractionState.ReadyToStartListening}>
									Πές μου
								</button>
								<button onClick={stopListening} disabled={interactionState != InteractionState.Listening}>
									Αποστολή
								</button>
								<button onClick={repeatQuestion}>
									Επανάληψη
								</button>
								{import.meta.env.VITE_ENV =='development' &&
									<div>
										<h2>Transcript:</h2>
										<p>{transcript}</p>
									</div>
								}
							</div>							
						</div>
					</div>
				}
			</div>
		</>
	)
}

export default Chat
