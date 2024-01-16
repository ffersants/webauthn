import React, { useState } from "react";
import Register from "./appresentation/pages/register";
import Login from "./appresentation/pages/login";
import GerenciarDispositivos from "./appresentation/pages/gerenciar-dispositivos";

const App = () => {
	return (
		<>
			<Register/> 
			<Login/>
			<GerenciarDispositivos/>
		</>
	)
};

export default App;
