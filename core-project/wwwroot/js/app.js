const uri = '/LibraryBook';
let books = [];

function getAuthHeaders() {
    const token = sessionStorage.getItem('jwtToken');
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}` 
    };
}

async function getItems() {
    console.log("מנסה למשוך נתונים מהשרת...");
    try {
        const response = await fetch(uri, {
            method: 'GET',
            headers: getAuthHeaders() 
        });

        if (response.status === 401) {
            alert("לא מחובר! אנא התחבר מחדש.");
            window.location.href = "login.html";
            return;
        }

        const data = await response.json();
        books = data;
        _displayItems(books);
    } catch (error) {
        console.error("שגיאה במשיכת נתונים:", error);
    }
}

async function addItem() {
    const nameInput = document.getElementById('bookName');
    const authorInput = document.getElementById('authorName');
    const borrowedInput = document.getElementById('isBorrowed');
    const adultsInput = document.getElementById('IsForAdults');

    const item = {
        Name: nameInput.value.trim(),
        WriterName: authorInput.value.trim(),
        IsBorrowed: borrowedInput.checked,
        IsForAdults: adultsInput.checked
    };

    const response = await fetch(uri, {
        method: 'POST',
        headers: getAuthHeaders(), 
        body: JSON.stringify(item)
    });

    if (response.ok) {
        await getItems();
        clearAddForm();
    } else if (response.status === 403) {
        alert("אין לך הרשאות להוסיף ספרים!");
    } else if (response.status === 401) {
        window.location.href = "login.html";
    }
}

function _displayItems(data) {
    const tbody = document.getElementById('books');
    tbody.innerHTML = '';
    
    const role = sessionStorage.getItem('userRole'); 

    data.forEach(item => {
        let tr = tbody.insertRow();
        
        tr.insertCell(0).innerText = item.Id;
        tr.insertCell(1).innerText = item.Name;
        tr.insertCell(2).innerText = item.WriterName;
        tr.insertCell(3).innerText = item.IsBorrowed ? 'Yes' : 'No';
        tr.insertCell(4).innerText = item.IsForAdults ? 'Yes' : 'No';

        let tdActions = tr.insertCell(5);
        if (role === 'Admin') {
            tdActions.innerHTML = `
                <button onclick="displayEditForm(${item.Id})">✏️ Edit</button>
                <button onclick="deleteItem(${item.Id})">🗑️ Delete</button>
            `;
            tdActions.style.textAlign = 'center';
            tdActions.style.display = 'flex';
            tdActions.style.justifyContent = 'center';
            tdActions.style.gap = '8px';
            tdActions.style.flexWrap = 'wrap';
        }
    });
}

async function deleteItem(id) {
    if (!confirm("האם את בטוחה שברצונך למחוק את הספר?")) return;

    try {
        const response = await fetch(`${uri}/${id}`, {
            method: 'DELETE',
            headers: getAuthHeaders() 
        });

        if (response.ok) {
            await getItems();
        } else if (response.status === 403) {
            alert("אין לך הרשאות למחוק ספרים!");
        } else if (response.status === 401) {
            window.location.href = "login.html";
        }
    } catch (error) {
        console.error("שגיאה במחיקה:", error);
    }
}

async function updateBook() {
    const itemId = parseInt(document.getElementById('edit-id').value, 10);
    
    const item = {
        Id: itemId,
        Name: document.getElementById('edit-name').value.trim(),
        WriterName: document.getElementById('edit-author').value.trim(),
        IsBorrowed: document.getElementById('edit-isBorrowed').checked,
        IsForAdults: document.getElementById('edit-isForAdults').checked
    };

    try {
        const response = await fetch(`${uri}/${itemId}`, {
            method: 'PUT',
            headers: getAuthHeaders(), 
            body: JSON.stringify(item)
        });

        if (response.ok) {
            closeEditForm();
            await getItems();
        } else if (response.status === 403) {
            alert("אין לך הרשאות לעדכן ספרים!");
        } else if (response.status === 401) {
            window.location.href = "login.html";
        }
    } catch (error) {
        console.error("שגיאה בעדכון:", error);
    }
}

function displayEditForm(id) {
    const book = books.find(b => b.Id === id);
    if (!book) {
        alert('ספר לא נמצא');
        return;
    }
    
    document.getElementById('edit-id').value = book.Id;
    document.getElementById('edit-name').value = book.Name;
    document.getElementById('edit-author').value = book.WriterName;
    document.getElementById('edit-isBorrowed').checked = book.IsBorrowed;
    document.getElementById('edit-isForAdults').checked = book.IsForAdults;
    
    document.getElementById('editFormContainer').classList.add('show');
}

function closeEditForm() {
    document.getElementById('editFormContainer').classList.remove('show');
}

function clearAddForm() {
    document.getElementById('bookName').value = '';
    document.getElementById('authorName').value = '';
    document.getElementById('isBorrowed').checked = false;
    document.getElementById('IsForAdults').checked = false;
}

document.addEventListener('DOMContentLoaded', function() {
    getItems();
    displayUserProfile();
});

function displayUserProfile() {
    const token = sessionStorage.getItem('jwtToken');
    const picture = sessionStorage.getItem('userProfilePicture');
    
    console.log('displayUserProfile called');
    console.log('Token exists:', !!token);
    console.log('Picture URL:', picture);
    
    if (!token) {
        console.log('No token, redirecting to login');
        window.location.href = "login.html";
        return;
    }

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

function createInitialsAvatar(name, parentElement) {
    const initials = name
        .split(' ')
        .map(word => word.charAt(0).toUpperCase())
        .join('')
        .substring(0, 2);
    
    const colors = ['#FF6B6B', '#4ECDC4', '#45B7D1', '#FFA07A', '#98D8C8', '#F38181'];
    const bgColor = colors[Math.floor(Math.random() * colors.length)];
    
    const oldAvatar = parentElement.querySelector('img, svg');
    if (oldAvatar) {
        oldAvatar.remove();
    }
    
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
    
    const userNameSpan = parentElement.querySelector('span');
    if (userNameSpan) {
        userNameSpan.parentNode.insertBefore(svg, userNameSpan);
    } else {
        parentElement.insertBefore(svg, parentElement.firstChild);
    }
}

function logout() {
    console.log('Logout called');
    sessionStorage.clear();
    window.location.href = "login.html";
}