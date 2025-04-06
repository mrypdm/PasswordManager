function showPassword(passwordBoxId) {
    let passwordBox = document.getElementById(passwordBoxId);
    if (passwordBox.type === "password") {
        passwordBox.type = "text";
    } else {
        passwordBox.type = "password";
    }
}

function copyToClipboard(boxId) {
    let box = document.getElementById(boxId);
    box.select();
    box.setSelectionRange(0, 99999); // For mobile devices
    navigator.clipboard.writeText(box.value);
}

function buildCheckStatus(checkStatus) {
    let isCompomised = checkStatus.isCompomised ? " " : " not";
    return `Password is${isCompomised} compomised and password strength is ${checkStatus.strength}`
}

function getCsrfTokenHeader() {
    return {
        "X-CSRF-TOKEN": $('input[name="__RequestVerificationToken"]').val()
    }
}
