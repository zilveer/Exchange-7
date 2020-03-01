"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.authConfig = {
    // Url of the Identity Provider
    issuer: 'https://localhost:44326',
    // URL of the SPA to redirect the user to after login
    redirectUri: window.location.origin + '/',
    responseType: 'code',
    // The SPA's id. The SPA is registered with this id at the auth-server
    clientId: 'brvesqplaedoadrhklar',
    // set the scope for the permissions the client should request
    // The first three are defined by OIDC. The 4th is a usecase-specific one
    scope: 'openid orders:read',
};
//# sourceMappingURL=authConfig.js.map