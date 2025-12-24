const API_URL = '/api/Users';

// פונקציה ראשית להרצה עם טעינת הדף
document.addEventListener('DOMContentLoaded', () => {
    loadUsers();
    setupFormListener();
});

// 1. שליפת כל המשתמשים והצגתם בטבלה
async function loadUsers() {
    try {
        const response = await fetch(API_URL);
        if (!response.ok) throw new Error('נכשל בטעינת נתונים');
        
        const users = await response.json();
        const tbody = document.getElementById('userTableBody');
        tbody.innerHTML = '';

        users.forEach(user => {
            const row = tbody.insertRow();
            row.innerHTML = `
                <td>${user.Id}</td>
                <td>${user.Name}</td>
                <td>${user.Password}</td>
                <td>
                   <button onclick="openEditForm(${user.Id}, '${user.Name}')">ערוך</button>
        <button onclick="deleteUser(${user.Id})">מחק</button>
                </td>
            `;
        });
    } catch (error) {
        console.error('Error:', error);
        document.getElementById('listMessage').innerText = 'שגיאה בחיבור לשרת';
    }
}

// 2. הגדרת מאזין לטופס ההוספה
function setupFormListener() {
    const form = document.getElementById('addUserForm');
    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        const nameInput = document.getElementById('userName');
        const passInput = document.getElementById('userPassword');

        const userData = {
            Name: nameInput.value,
            Password: passInput.value
        };

        try {
            const response = await fetch(API_URL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(userData)
            });

            if (response.ok) {
                nameInput.value = '';
                passInput.value = '';
                loadUsers(); // רענון הטבלה
            } else {
                alert('שגיאה בשמירת המשתמש');
            }
        } catch (error) {
            console.error('Error:', error);
        }
    });
}

// 3. מחיקת משתמש לפי ID
async function deleteUser(id) {
    if (!confirm('האם אתה בטוח שברצונך למחוק?')) return;

    try {
        const response = await fetch(`${API_URL}/${id}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            loadUsers(); // רענון הטבלה
        } else {
            alert('לא ניתן למחוק את המשתמש');
        }
    } catch (error) {
        console.error('Error:', error);
    }
}
// פתיחת חלונית העריכה ומילוי נתונים קיימים
function openEditForm(id, name) {
    document.getElementById('editUserId').value = id;
    document.getElementById('editUserName').value = name;
    document.getElementById('editUserPassword').value = ''; // השארת סיסמה ריקה לעדכון
    
    // הצגת המודאל (שימוש ב-flex כפי שהגדרת ב-CSS למרכוז)
    document.getElementById('editFormContainer').style.display = 'flex';
}

// סגירת החלונית
function closeEditForm() {
    document.getElementById('editFormContainer').style.display = 'none';
}

// שמירת השינויים ושליחה לשרת
async function saveUserEdit() {
    const id = document.getElementById('editUserId').value;
    const name = document.getElementById('editUserName').value;
    const password = document.getElementById('editUserPassword').value;

    const updatedUser = {
        Id: parseInt(id),
        Name: name,
        Password: password
    };

    try {
        const response = await fetch(`${API_URL}/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(updatedUser)
        });

        if (response.ok) {
            closeEditForm();
            loadUsers(); // רענון הטבלה
        } else {
            alert('שגיאה בעדכון המשתמש');
        }
    } catch (error) {
        console.error('Error updating user:', error);
    }
}