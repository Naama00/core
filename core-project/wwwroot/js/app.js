const uri = '/LibraryBook'; // נתיב יחסי בדרך כלל עובד הכי טוב בתוך פרויקט Core
let books = [];

async function getItems() {
    console.log("מנסה למשוך נתונים מהשרת...");
    try {
        const response = await fetch(uri);
        const data = await response.json();
        console.log("נתונים שהתקבלו:", data);
        books = data;
        _displayItems(books);
    } catch (error) {
        console.error("שגיאה במשיכת נתונים:", error);
    }
}

async function addItem() {
    const item = {
        Name: document.getElementById('bookName').value.trim(),      // N גדולה
        WriterName: document.getElementById('authorName').value.trim(), // W גדולה
        IsBorrowed: document.getElementById('isBorrowed').checked,
        IsForAdults: document.getElementById('IsForAdults').checked
    };

    const response = await fetch(uri, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(item)
    });

    if (response.ok) {
        // כאן הקסם: אנחנו קוראים שוב לשרת כדי להביא את הרשימה המעודכנת
        await getItems(); 
        clearAddForm();
    }
}
function _displayItems(data) {
    const tbody = document.getElementById('books');
    tbody.innerHTML = ''; // ניקוי הטבלה

    data.forEach(item => {
        let tr = tbody.insertRow();
        
        // שימוש באותיות גדולות בדיוק כפי שמופיע אצלך בקונסול!
        tr.insertCell(0).innerText = item.Id;
        tr.insertCell(1).innerText = item.Name;
        tr.insertCell(2).innerText = item.WriterName;
        tr.insertCell(3).innerText = item.IsBorrowed ? 'Yes' : 'No';
        tr.insertCell(4).innerText = item.IsForAdults ? 'Yes' : 'No';

        // כפתור עריכה
        let tdEdit = tr.insertCell(5);
        tdEdit.innerHTML = `<button onclick="displayEditForm(${item.Id})">Edit</button>`;

        // כפתור מחיקה
        let tdDelete = tr.insertCell(6);
        tdDelete.innerHTML = `<button onclick="deleteItem(${item.Id})">Delete</button>`;
    });
}
function clearAddForm() {
    document.getElementById('bookName').value = '';
    document.getElementById('authorName').value = '';
    document.getElementById('isBorrowed').checked = false;
    document.getElementById('IsForAdults').checked = false;
}
async function deleteItem(id) {
    if (!confirm("האם את בטוחה שברצונך למחוק את הספר?")) return;

    try {
        const response = await fetch(`${uri}/${id}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            console.log("הספר נמחק בהצלחה");
            await getItems(); // רענון הטבלה
        } else {
            console.error("שגיאה במחיקה");
        }
    } catch (error) {
        console.error("נכשל בחיבור לשרת למחיקה:", error);
    }
}
function displayEditForm(id) {
    const item = books.find(item => (item.Id || item.id) === id);

    if (item) {
        document.getElementById('edit-id').value = item.Id || item.id;
        document.getElementById('edit-name').value = item.Name || item.name;
        document.getElementById('edit-author').value = item.WriterName || item.writerName;
        document.getElementById('edit-isBorrowed').checked = item.IsBorrowed || item.isBorrowed;
        document.getElementById('edit-isForAdults').checked = item.IsForAdults || item.isForAdults;
        
        // כאן השינוי: משתמשים ב-flex כדי שהמרכוז מה-CSS יפעל
        document.getElementById('editFormContainer').style.display = 'flex';
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
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(item)
        });

        if (response.ok) {
            closeEditForm(); // סגירת הטופס
            await getItems(); // רענון הטבלה
        } else {
            console.error("שגיאה בעדכון הספר");
        }
    } catch (error) {
        console.error("שגיאה בשליחת עדכון:", error);
    }
}

function closeEditForm() {
    document.getElementById('editFormContainer').style.display = 'none';
}