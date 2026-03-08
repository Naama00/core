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
    displayUserProfile();
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
            const uemail = user.email || user.Email || '';
            const upass = user.password || user.Password;

            let actions = `<button onclick="openEditForm(${uid}, '${uname}')">✏️ Edit</button>`;
            const role = sessionStorage.getItem('userRole');
            if (role === 'Admin') {
                actions += ` <button onclick="deleteUser(${uid})">🗑️ Delete</button>`;
            }

            row.innerHTML = `
                <td>${uid}</td>
                <td>${uname}</td>
                <td>${uemail}</td>
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

        const nameInput = document.getElementById('newUserName');
        const emailInput = document.getElementById('newUserEmail');
        const passInput = document.getElementById('newUserPassword');

        const userData = {
            Name: nameInput.value,
            Email: emailInput.value,
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
                emailInput.value = '';
                passInput.value = '';
                loadUsers();
            } else if (response.status === 403) {
                alert('אין לך הרשאות להוסיף משתמשים!');
            } else if (response.status === 401) {
                window.location.href = "login.html";
            } else {
                const errorData = await response.text();
                console.error('POST error:', errorData);
                alert('שגיאה בשמירת המשתמש: ' + response.status);
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
    
    document.getElementById('editFormContainer').classList.add('show');
}

// סגירת החלונית
function closeEditForm() {
    document.getElementById('editFormContainer').classList.remove('show');
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
            const errorData = await response.text();
            console.error('Update error:', errorData);
            alert('שגיאה בעדכון המשתמש: ' + response.status);
        }
    } catch (error) {
        console.error('Error updating user:', error);
    }
}

// תצוגת פרופיל המשתמש
function displayUserProfile() {
    const token = sessionStorage.getItem('jwtToken');
    const picture = sessionStorage.getItem('userProfilePicture');
    
    console.log('displayUserProfile called (user.js)');
    console.log('Token exists:', !!token);
    console.log('Picture URL:', picture);
    
    if (!token) {
        console.log('No token, redirecting to login');
        window.location.href = "login.html";
        return;
    }
    
    // קחה שם המשתמש והצגה
    const userNameSpan = document.getElementById('userName');
    let userName = 'User';
    
    try {
        const base64Url = token.split('.')[1];
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const jsonPayload = decodeURIComponent(atob(base64).split('').map(function(c) {
            return '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2);
        }).join(''));
        const decoded = JSON.parse(jsonPayload);
        userName = decoded['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] || 'User';
        console.log('User name from token:', userName);
        if (userNameSpan) {
            userNameSpan.textContent = userName;
        }
    } catch (e) {
        console.error('Error decoding token:', e);
        if (userNameSpan) {
            userNameSpan.textContent = 'User';
        }
    }
    
    // הצגת תמונת הפרופיל אם קיימת
    const avatarContainer = document.getElementById('userProfile');
    console.log('Avatar container found:', !!avatarContainer);
    
    if (avatarContainer && picture) {
        console.log('Attempting to set avatar image to:', picture);
        const img = document.getElementById('userAvatar');
        if (img) {
            img.src = picture;
            img.style.display = 'block';
            img.onerror = function() {
                console.log('Image failed to load, creating initials avatar');
                createInitialsAvatar(userName, avatarContainer);
            };
        }
    } else if (avatarContainer) {
        console.log('No picture URL, creating initials avatar');
        createInitialsAvatar(userName, avatarContainer);
    }
}

// Create an avatar with initials
function createInitialsAvatar(name, parentElement) {
    const initials = name
        .split(' ')
        .map(word => word.charAt(0).toUpperCase())
        .join('')
        .substring(0, 2);
    
    const colors = ['#FF6B6B', '#4ECDC4', '#45B7D1', '#FFA07A', '#98D8C8', '#F38181'];
    const bgColor = colors[Math.floor(Math.random() * colors.length)];
    
    // Find or remove old avatar
    const oldAvatar = parentElement.querySelector('img, svg');
    if (oldAvatar) {
        oldAvatar.remove();
    }
    
    // Create SVG avatar
    const svg = document.createElementNS('http://www.w3.org/2000/svg', 'svg');
    svg.setAttribute('width', '40');
    svg.setAttribute('height', '40');
    svg.setAttribute('viewBox', '0 0 40 40');
    svg.setAttribute('style', 'border-radius: 50%; display: block;');
    
    const rect = document.createElementNS('http://www.w3.org/2000/svg', 'rect');
    rect.setAttribute('width', '40');
    rect.setAttribute('height', '40');
    rect.setAttribute('fill', bgColor);
    rect.setAttribute('rx', '50%');
    
    const text = document.createElementNS('http://www.w3.org/2000/svg', 'text');
    text.setAttribute('x', '50%');
    text.setAttribute('y', '50%');
    text.setAttribute('font-size', '16');
    text.setAttribute('font-weight', 'bold');
    text.setAttribute('fill', 'white');
    text.setAttribute('dominant-baseline', 'middle');
    text.setAttribute('text-anchor', 'middle');
    text.textContent = initials;
    
    svg.appendChild(rect);
    svg.appendChild(text);
    
    // Insert before the userName span
    const userNameSpan = parentElement.querySelector('span');
    if (userNameSpan) {
        userNameSpan.parentNode.insertBefore(svg, userNameSpan);
    } else {
        parentElement.insertBefore(svg, parentElement.firstChild);
    }
}

// Logout function
function logout() {
    console.log('Logout called');
    sessionStorage.clear();
    window.location.href = "login.html";
}