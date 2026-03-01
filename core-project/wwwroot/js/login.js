document.getElementById('loginForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    const usernameInput = document.getElementById('username').value;
    const passwordInput = document.getElementById('password').value;
    const message = document.getElementById('message');

    try {
        // 1. שליחה לשרת לבדיקת משתמש וקבלת טוקן
        const response = await fetch('/api/Auth/login', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json' // חובה!
            },
            // ודא שמות שדות תואמים ל-C#
            body: JSON.stringify({ Name: usernameInput, Password: passwordInput })
        });

        if (response.ok) {
            const data = await response.json();
            
            // 2. שמירת הטוקן ב-Session Storage
            sessionStorage.setItem('jwtToken', data.Token);
            sessionStorage.setItem('userRole', data.Role);

            message.style.color = "green";
            message.textContent = "התחברת בהצלחה!";
            window.location.href = "index.html";
        } else {
            message.style.color = "red";
            message.textContent = "שם משתמש או סיסמה שגויים.";
        }
    } catch (error) {
        console.error("שגיאה בהתחברות:", error);
        message.style.color = "red";
        message.textContent = "שגיאה בחיבור לשרת.";
    }
});