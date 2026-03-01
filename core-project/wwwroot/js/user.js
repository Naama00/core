const API_URL = 'http://localhost:5041/api/Users';

// פונקציית עזר להוספת הטוקן ל-Headers
function getAuthHeaders() {
    const token = sessionStorage.getItem('jwtToken');
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    };
}

// פונקציה ראשית להרצה עם טעינת הדף
document.addEventListener('DOMContentLoaded', () => {
    // בדיקת תפקיד כדי להסתיר אלמנטים לא מורשים
    const role = sessionStorage.getItem('userRole');
    if (role !== 'Admin') {
        // משתמש רגיל לא יכול להוסיף משתמשים
        const addForm = document.getElementById('addUserForm');
        if (addForm) addForm.style.display = 'none';
    }

    loadUsers();
    setupFormListener();
});

// 1. שליפת כל המשתמשים והצגתם בטבלה
async function loadUsers() {
    try {
        const response = await fetch(API_URL, {
            method: 'GET',
            headers: getAuthHeaders()
        });

        if (response.status === 401) {
            alert("לא מחובר! אנא התחבר מחדש.");
            window.location.href = "login.html";
            return;
        }

        if (!response.ok) throw new Error('נכשל בטעינת נתונים');
        
        const users = await response.json();
        const tbody = document.getElementById('userTableBody');
        
        // בדיקת בטיחות למקרה שהאלמנט לא נמצא ב-HTML
        if (!tbody) {
            console.error("Missing element: userTableBody");
            return;
        }

        tbody.innerHTML = '';

        users.forEach(user => {
            const row = tbody.insertRow();
            const uid = user.id || user.Id;
            const uname = user.name || user.Name;
            const upass = user.password || user.Password;

            let actions = `<button onclick="openEditForm(${uid}, '${uname}')">ערוך</button>`;
            const role = sessionStorage.getItem('userRole');
            if (role === 'Admin') {
                actions += ` <button onclick="deleteUser(${uid})">מחק</button>`;
            }

            row.innerHTML = `
                <td>${uid}</td>
                <td>${uname}</td>
                <td>${upass}</td>
                <td>${actions}</td>
            `;
        });
    } catch (error) {
        console.error('Error:', error);
        alert('שגיאה בטעינת המשתמשים. בדקי שהשרת פועל.');
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
                headers: getAuthHeaders(),
                body: JSON.stringify(userData)
            });

            if (response.ok) {
                nameInput.value = '';
                passInput.value = '';
                loadUsers();
            } else if (response.status === 403) {
                alert('אין לך הרשאות להוסיף משתמשים!');
            } else if (response.status === 401) {
                window.location.href = "login.html";
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
            method: 'DELETE',
            headers: getAuthHeaders()
        });

        if (response.ok) {
            loadUsers();
        } else if (response.status === 403) {
            alert('אין לך הרשאות למחוק משתמשים!');
        } else if (response.status === 401) {
            window.location.href = "login.html";
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
    document.getElementById('editUserPassword').value = '';
    
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
            headers: getAuthHeaders(),
            body: JSON.stringify(updatedUser)
        });

        if (response.ok) {
            closeEditForm();
            loadUsers();
        } else if (response.status === 403) {
            alert('אין לך הרשאות לעדכן משתמשים אחרים!');
        } else if (response.status === 401) {
            window.location.href = "login.html";
        } else {
            alert('שגיאה בעדכון המשתמש');
        }
    } catch (error) {
        console.error('Error updating user:', error);
    }
}