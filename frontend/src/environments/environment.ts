export const environment = {
	production: false,
	apiUrl: 'http://localhost:5071/api',
	baseUrl: 'http://localhost:5071',
	rtcIceServers: [
		{ urls: 'stun:stun.l.google.com:19302' },
		{ urls: 'stun:stun1.l.google.com:19302' }
		// Add TURN for cross-network reliability:
		// { urls: 'turn:turn.yourdomain.com:3478', username: 'user', credential: 'pass' }
	],
	rtcForceRelay: false
};
