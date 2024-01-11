import { startAuthentication } from '@simplewebauthn/browser';
import React from 'react';

const Login = () => {
  const elemBegin = React.useRef(null);
  const elemSuccess = React.useRef(null);
  const elemError = React.useRef(null);

  const handleClick = async () => {
    (elemSuccess as any).current.innerHTML = '';
    (elemError as any).current.innerHTML = '';

    try {
      const resp = await fetch('http://localhost:5139/api/login/get-options?Username=user@example.com&displayName=string', {
        credentials: "include"
      });

      const asseResp = await startAuthentication(await resp.json());
			const cookie = document.cookie;

      const verificationResp = await fetch('http://localhost:5139/api/login/assert-options', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Cookie': cookie
        },
        body: JSON.stringify({
          attestationResponse: asseResp,
          username: "user@example.com",
					displayName: "string",
        }),
        credentials: "include"
      });

      const verificationJSON = await verificationResp.json();

      if (verificationJSON && verificationJSON.verified) {
        (elemSuccess as any).current.innerHTML = 'Success!';
      } else {
        (elemError as any).current.innerHTML = `Oh no, something went wrong! Response: <pre>${JSON.stringify(
          verificationJSON,
        )}</pre>`;
      }
    } catch (error) {
        (elemError as any).current.innerText = error;
      throw error;
    }
  };

  React.useEffect(() => {
    (elemBegin as any).current.addEventListener('click', handleClick);
    return () => {
        (elemBegin as any).current.removeEventListener('click', handleClick);
    };
  }, []);

  return (
    <div>
      <button ref={elemBegin} id="btnBegin">
        Begin Authentication
      </button>
      <div ref={elemSuccess} id="success"></div>
      <div ref={elemError} id="error"></div>
    </div>
  );
};

export default Login;
