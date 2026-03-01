const uri = '/LibraryBook';
let books = [];

// --- פונקציית עזר להוספת הטוקן ל-Headers ---
function getAuthHeaders() {
    const token = sessionStorage.getItem('jwtToken');
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}` // שליחת הטוקן
    };
}
// -------------------------------------------

async function getItems() {
    console.log("מנסה למשוך נתונים מהשרת...");
    try {
        const response = await fetch(uri, {
            method: 'GET',
            headers: getAuthHeaders() // הוספת Headers לקריאה
        });

        if (response.status === 401) {
            alert("לא מחובר! אנא התחבר מחדש.");
            window.location.href = "login.html"; // חזרה לדף התחברות
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
    // מומלץ לתפוס אלמנטים פעם אחת מחוץ לפונקציה, אבל זה בסדר
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
        headers: getAuthHeaders(), // שימוש בפונקציית העזר
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
    
    // קבלת התפקיד כדי לדעת אם להציג כפתורי עריכה/מחיקה
    const role = sessionStorage.getItem('userRole'); 

    data.forEach(item => {
        let tr = tbody.insertRow();
        
        tr.insertCell(0).innerText = item.Id;
        tr.insertCell(1).innerText = item.Name;
        tr.insertCell(2).innerText = item.WriterName;
        tr.insertCell(3).innerText = item.IsBorrowed ? 'Yes' : 'No';
        tr.insertCell(4).innerText = item.IsForAdults ? 'Yes' : 'No';

        // הוספת כפתורים רק אם המשתמש הוא מנהל
        let tdActions = tr.insertCell(5);
        if (role === 'Admin') {
            // שימוש ב-template literals ליצירת הכפתורים
            tdActions.innerHTML = `
                <button onclick="displayEditForm(${item.Id})">Edit</button>
                <button onclick="deleteItem(${item.Id})">Delete</button>
            `;
        }
    });
}

async function deleteItem(id) {
    if (!confirm("האם את בטוחה שברצונך למחוק את הספר?")) return;

    try {
        const response = await fetch(`${uri}/${id}`, {
            method: 'DELETE',
            headers: getAuthHeaders() // הוספת הטוקן
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
            headers: getAuthHeaders(), // הוספת הטוקן
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

function closeEditForm() {
    document.getElementById('editFormContainer').style.display = 'none';
}

function clearAddForm() {
    document.getElementById('bookName').value = '';
    document.getElementById('authorName').value = '';
    document.getElementById('isBorrowed').checked = false;
    document.getElementById('IsForAdults').checked = false;
}

// טעינת הנתונים כשדף הספרייה נפתח
document.addEventListener('DOMContentLoaded', getItems);