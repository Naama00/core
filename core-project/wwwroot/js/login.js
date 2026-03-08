// המתן עד שה-DOM הטען בעצמו
console.log("login.js עומס");

const form = document.getElementById('loginForm');
console.log("form element:", form);

if (form) {
    form.addEventListener('submit', async function (e) {
        e.preventDefault();
        console.log("Form submit event triggered");

        const emailElement = document.getElementById('email');
        const passwordElement = document.getElementById('password');
        const messageDiv = document.getElementById('message');

        console.log("Email element found:", emailElement ? "Yes" : "No");
        console.log("Password element found:", passwordElement ? "Yes" : "No");

        if (!emailElement || !passwordElement) {
            console.error("Missing form elements!");
            return;
        }

        const email = emailElement.value.trim();
        const password = passwordElement.value.trim();

        console.log("Sending login request with email:", email);

        try {
            const response = await fetch('/api/Auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ Email: email, Password: password })
            });

            if (response.ok) {
                const data = await response.json();
                sessionStorage.setItem('jwtToken', data.Token);
                sessionStorage.setItem('userRole', data.Role);
                
                messageDiv.style.color = "green";
                messageDiv.textContent = "התחברת בהצלחה!";
                
                setTimeout(() => {
                    window.location.href = "index.html";
                }, 500);
            } else {
                messageDiv.style.color = "red";
                messageDiv.textContent = "אימייל או סיסמה שגויים";
                console.error("Login failed:", response.status);
            }
        } catch (error) {
            console.error("Network error:", error);
            messageDiv.style.color = "red";
            messageDiv.textContent = "שגיאה בהתחברות לשרת";
        }
    });
} else {
    console.error("loginForm not found in DOM");
}