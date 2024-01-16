import httpClient from "../httpClient"

const deleteDispositivoCadastrado = async (chavePublicaId: string) => {
    await httpClient.delete(`/dispositivos`, {
        data: {
            chavePublicaId
        }
    })
}

export default deleteDispositivoCadastrado