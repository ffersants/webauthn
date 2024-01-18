import { startRegistration } from "@simplewebauthn/browser";

import { useState } from "react";
import usuario from "../../domain/constants";
const Register = () => {
	const [errorMessage, setErrorMessage] = useState("");
	const [successMessage, setSuccessMessage] = useState("");
	const baseUrl = process.env.REACT_APP_API_URL;

	const handleRegistration = async () => {
		// Reset success/error messages
		setSuccessMessage("");
		setErrorMessage("");

		try {
			// GET registration options from the endpoint that calls
			const resp = await fetch(baseUrl + "/register/get-options", {
				body: JSON.stringify({
					username: usuario.matricula,
					displayName: usuario.email,
					attestationResponse: "string",
				}),
				method: "POST",
				headers: {
					"Content-Type": "application/json",
				},
				credentials: "include",
			});
			const options = await resp.json();
			const cookie = document.cookie;

			// Pass the options to the authenticator and wait for a response
			const attResp = await startRegistration(options);

			// POST the response to the endpoint that calls
			const verificationResp = await fetch(
				baseUrl + "/register/assert-options",
				{
					method: "POST",
					headers: {
						"Content-Type": "application/json",
						Cookie: cookie,
					},
					body: JSON.stringify({
						AuthenticatorAttestationRawResponse: attResp,
						username: usuario.matricula,
						displayName: usuario.email,
						userAgent: navigator.userAgent,
						deviceInfo: {
							userAgent: navigator.userAgent,
							platform: navigator.platform,
							appName: navigator.appName,
							appVersion: navigator.appVersion,
						},
					}),
					credentials: "include",
				}
			);

			// Wait for the results of verification
			const verificationJSON = await verificationResp.json();

			// Show UI appropriate for the `verified` status
			if (verificationJSON && verificationJSON.verified) {
				setSuccessMessage("Success!");
			} else {
				setErrorMessage(
					`Oh no, something went wrong! Response: ${JSON.stringify(
						verificationJSON
					)}`
				);
			}
		} catch (error) {
			// Some basic error handling
			if ((error as any).name === "InvalidStateError") {
				setErrorMessage(
					"Error: Authenticator was probably already registered by user"
				);
			} else {
				setErrorMessage((error as any).message || "An error occurred");
			}
		}
	};

	return (
		<div>
			<button id='btnBegin' onClick={handleRegistration}>
				Begin Registration
			</button>
			<div id='success'>{successMessage}</div>
			<div id='error'>{errorMessage}</div>
		</div>
	);
};

export default Register;
