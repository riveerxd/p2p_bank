const URL = import.meta.env.VITE_SHUTDOWN_URL || '';

export async function shutdownServer() {
    return await fetch(URL, {
        method: 'GET',
    });
}