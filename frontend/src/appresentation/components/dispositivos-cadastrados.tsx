import axios from "axios";
import { useEffect, useState } from "react";
import usuario from "../../domain/constants";
import { DispositivoCadastrado } from "../../domain/entities/dispositivo-cadastrado";
import getDispositivosCadastrados from "../../services/dispositivos/get-dispositivos-cadastrados";
import deleteDispositivoCadastrado from "../../services/dispositivos/delete-dispositivo-cadastrado";

const DispositivosCadastrados = () => {
	const [dispositivos, setDispositivos] = useState<DispositivoCadastrado[]>([]);

	const getData = async () => {
		const result = await getDispositivosCadastrados(usuario.matricula);
		setDispositivos(result);
	};

	useEffect(() => {
		getData();
	}, []);

	return (
		<div>
			{dispositivos.map((i) => (
				<div>
					<p>{i.dadosDispositivo}</p>
					<button
						onClick={() =>
							deleteDispositivoCadastrado(i.chavePublicaId).then(() =>
								getData()
							)
						}
					>
						X
					</button>
				</div>
			))}
		</div>
	);
};

export default DispositivosCadastrados;
