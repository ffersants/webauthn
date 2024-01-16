import usuario from "../../domain/constants"
import { DispositivoCadastrado } from "../../domain/entities/dispositivo-cadastrado"
import httpClient from "../httpClient"

const getDispositivosCadastrados = async (matriculaDoUsuario: string) => {
    const result = await httpClient.get(`/dispositivos/${matriculaDoUsuario}`)
    return result.data as DispositivoCadastrado[]
}

export default getDispositivosCadastrados