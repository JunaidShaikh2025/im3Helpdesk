export const environment = {
	production: false,
	apiUrl: 'https://localhost:7071/api',
	baseUrl: 'https://localhost:7071',
	rtcIceServers: [
		{ urls: 'stun:stun.l.google.com:19302' },
		{ urls: 'stun:stun1.l.google.com:19302' }
		// Add TURN for cross-network reliability:
		// { urls: 'turn:turn.yourdomain.com:3478', username: 'user', credential: 'pass' }
	],
	rtcForceRelay: false
};
