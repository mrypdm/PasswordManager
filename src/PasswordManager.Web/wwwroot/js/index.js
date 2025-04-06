async function getHeaders() {
    let table = document.getElementById("items-table")

    let response = null
    try {
        response = await send("api/account/headers", "GET", null, getCsrfTokenHeader())
        response = await response.json();
    } catch (error) {
        let text = await response.text();
        alert(text);
        return
    }

    for (let i = 0; i < response.length; i++) {
        let row = table.insertRow(i+1);
        var idCell = row.insertCell(0);
        var nameCell = row.insertCell(1);
        var copyUserCell = row.insertCell(2);
        var copyPasswordCell = row.insertCell(3);
        var viewCell = row.insertCell(4);

        idCell.innerHTML = response[i].id;
        nameCell.innerHTML = response[i].name;
        copyUserCell.innerHTML = `<input style="width: auto" type="button" value="Copy login" onclick="getAccountDataAndCopy('${response[i].id}', 'login')" />`
        copyPasswordCell.innerHTML = `<input style="width: auto" type="button" value="Copy password" onclick="getAccountDataAndCopy('${response[i].id}', 'password')" />`
        viewCell.innerHTML = `<input style="width: auto" type="button" value="View" onclick="location.replace('/account?id=${response[i].id}')" />`
    }
}

async function getAccountDataAndCopy(accountId, property) {
    try {
        let response = await getAccountDataRaw(accountId);
        navigator.clipboard.writeText(response[property]);
    } catch (response) {
        let text = await response.text();
        alert(text);
    }
}
