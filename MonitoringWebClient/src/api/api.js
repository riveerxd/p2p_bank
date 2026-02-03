const URL = import.meta.env.VITE_SERVER_ADDRESS || '';

export async function shutdownServer() {
    return await fetch(`${URL}/shutdown`, {
        method: 'GET',
    });
}

export async function getStartTime() {
    return await fetch(`${URL}/starttime`, {
        method: 'GET',
    });
}