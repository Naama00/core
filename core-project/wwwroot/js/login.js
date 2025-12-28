document.getElementById('loginForm').addEventListener('submit', function(e) {
    e.preventDefault();

    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;
    const message = document.getElementById('message');

    // 1. שליפת המערך הקיים מה-LocalStorage
    let users = JSON.parse(localStorage.getItem('myAppUsers')) || [];

    // 2. בדיקה האם המשתמש כבר קיים
    const userIndex = users.findIndex(u => u.username === username);

    if (userIndex !== -1) {
        // המשתמש קיים - נבדוק סיסמה
        if (users[userIndex].password === password) {
            message.style.color = "green";
            message.textContent = "התחברת בהצלחה!";
            window.location.href = "index.html"; 
        } else {
            message.style.color = "red";
            message.textContent = "סיסמה שגויה.";
        }
    } else {
        // 3. המשתמש לא קיים - שמירה חדשה
        const newUser = { username: username, password: password };
        users.push(newUser);
        
        // עדכון ה-LocalStorage
        localStorage.setItem('myAppUsers', JSON.stringify(users));
        
        console.log("משתמש נשמר בהצלחה:", newUser); // בדיקה בקונסול
        message.style.color = "blue";
        message.textContent = "נרשמת בהצלחה ונשמרת במערכת!";
         window.location.href = "index.html"; 
    }
});