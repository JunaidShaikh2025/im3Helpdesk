export const environment = {
	production: true,
	apiUrl: 'https://dmwebapi-dmahdwf7c5c3e4cq.centralindia-01.azurewebsites.net/api',
	baseUrl: 'https://dmwebapi-dmahdwf7c5c3e4cq.centralindia-01.azurewebsites.net',
	rtcIceServers: [
		{ urls: 'stun:stun.l.google.com:19302' },
		{ urls: 'stun:stun1.l.google.com:19302' }
		// REQUIRED for Teams-like cross-network stability:
		// { urls: 'turn:turn.yourdomain.com:3478', username: 'user', credential: 'pass' }
	],
	rtcForceRelay: false
};
