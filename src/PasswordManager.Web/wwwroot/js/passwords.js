async function verifyPassword(passwordBoxId) {
    let passwordBox = document.getElementById(passwordBoxId);
    let data = {
        "Password": passwordBox.value
    };

    try {
        let response = await send("api/password/verify", "POST", data, getCsrfTokenHeader());
        response = await response.json();
        alert(buildCheckStatus(response));
    } catch (response) {
        let text = await response.text();
        alert(text);
    }
}

async function generatePassword() {
    let passwordBox = document.getElementById('password-generator-result');
    let passwordVerifyResult = document.getElementById('password-generator-verify-result');
    let lengthBox = document.getElementById("password-generator-length");
    let useLowerLettersBox = document.getElementById("password-generator-use-lower-letters");
    let useUpperLettersBox = document.getElementById("password-generator-use-upper-letters");
    let useNumbersBox = document.getElementById("password-generator-use-numbers");
    let useCharactersBox = document.getElementById("password-generator-specific-characters");

    let data = {
        "Length": lengthBox.value,
        "UseLowerLetters": useLowerLettersBox.checked,
        "UseUpperLetters": useUpperLettersBox.checked,
        "UseNumbers": useNumbersBox.checked,
        "SpecialSymbols": useCharactersBox.value
    }

    try {
        let response = await send("api/password/generate", "POST", data, getCsrfTokenHeader());
        response = await response.json();
        passwordBox.value = response.password;
        passwordVerifyResult.innerHTML = buildCheckStatus(response.checkStatus)
    } catch (response) {
        let text = await response.text();
        alert(text);
    }
}
