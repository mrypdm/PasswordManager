async function send(url, method, withResponse) {
    return await __send(url, { method: method }, withResponse);
}

async function send_json(url, method, body, withResponse) {
    return await __send(url, {
        method: method,
        body: JSON.stringify(body),
        headers: { "Content-Type": "application/json" },
    }, withResponse);
}

async function __send(url, data, withResponse) {
    let response = await fetch(url, data);

    if (response.ok) {
        if (withResponse) {
            return await response.json();
        }
        return response.status;
    }

    throw new Error(response.status);
}
