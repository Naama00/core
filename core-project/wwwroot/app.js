const uri = '/LibraryBook';
let books = [];

window.onload = function () {
    getItems();
};

function getItems() {
    return fetch(uri)
        .then(response => {
            if (!response.ok) throw new Error('Failed to load books');
            return response.json();
        })
        .then(data => {
            books = data;
            _displayItems(books);
        })
        .catch(error => console.error('Failed to get items', error));
}

function addItem() {
    const addNameTextbox = document.getElementById('bookName');
    const addAuthorTextbox = document.getElementById('authorName');
    const addIsBorrowedCheckbox = document.getElementById('isBorrowed');
    const addForAdultsCheckbox = document.getElementById('IsForAdults');

    if (!addNameTextbox.value.trim() || !addAuthorTextbox.value.trim()) {
        alert("Book name and author name are required.");
        return;
    }

    const item = {
        name: addNameTextbox.value.trim(),
        writerName: addAuthorTextbox.value.trim(),
        isBorrowed: addIsBorrowedCheckbox.checked,
        isForAdults: addForAdultsCheckbox.checked
    };

    console.log('POST', uri, item);
fetch(uri, {
    method: 'POST',
    headers: {
        'Accept': 'application/json',
        'Content-Type': 'application/json'
    },
    body: JSON.stringify(item)
})
.then(async response => {
    if (!response.ok) {
        const text = await response.text(); 
        throw new Error(`Failed to add book (${response.status}) ${text}`);
    }

    const newBook = await response.json();
    books.push(newBook);
    _displayItems(books);
})
.catch(error => {
    console.error('Failed to add item', error);
    alert('Failed to add book: ' + error.message);
})
.finally(() => {
    clearAddForm();
});

}

function clearAddForm() {
    document.getElementById('bookName').value = '';
    document.getElementById('authorName').value = '';
    document.getElementById('isBorrowed').checked = false;
    document.getElementById('IsForAdults').checked = false;
    document.getElementById('bookName').focus();
}

function deleteItem(id) {
    fetch(`${uri}/${id}`, {
        method: 'DELETE'
    })
    .then(() => getItems())
    .catch(error => console.error('Failed to delete item', error));
}

function displayEditForm(id) {
    const item = books.find(book => book.id === id);
    if (!item) return;

    const editPanel = document.getElementById('edit');
    if (editPanel) editPanel.style.display = 'block';

    const editFormElem = document.getElementById('editForm');
    if (editFormElem) editFormElem.style.display = 'block';

    document.getElementById('editId').value = item.id;
    document.getElementById('editBookName').value = item.name || '';
    document.getElementById('editAuthorName').value = item.writerName || '';
    document.getElementById('editIsForAdults').checked = !!item.isForAdults;
    document.getElementById('editIsBorrowed').checked = !!item.isBorrowed;

    document.getElementById('editBookName').focus();
    editPanel.scrollIntoView({ behavior: 'smooth', block: 'center' });
}

async function updateBook() {
    const id = document.getElementById('editId').value;
    if (!id) return false;

    const item = {
        id: Number(id),
        name: document.getElementById('editBookName').value.trim(),
        writerName: document.getElementById('editAuthorName').value.trim(),
        isForAdults: document.getElementById('editForAdults').checked
    };

    try {
        const response = await fetch(`${uri}/${id}`, {
            method: 'PUT',
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(item)
        });

        if (!response.ok) {
            const text = await response.text().catch(() => '');
            throw new Error(`Update failed: ${response.status} ${text}`);
        }

        cancelEdit(); // Hide the edit panel and reset
        await getItems(); // Refresh the items
    } catch (err) {
        console.error('Failed to update item', err);
        alert('Failed to update book: ' + err.message);
    }

    return false;
}

function cancelEdit() {
    const editPanel = document.getElementById('edit');
    if (editPanel) editPanel.style.display = 'none';
}

function _displayItems(data) {
    const tbody = document.getElementById('books');
    tbody.innerHTML = '';
    data.forEach(item => {
        let tr = tbody.insertRow();
        let tdId = tr.insertCell(0);
        tdId.innerText = item.id ?? '';
        let tdName = tr.insertCell(1);
        tdName.innerText = item.name ?? '';
        let tdWriter = tr.insertCell(2);
        tdWriter.innerText = item.writerName ?? '';
        let tdBorrowed = tr.insertCell(3);
        tdBorrowed.innerText = item.isBorrowed ? 'Yes' : 'No';
        let tdForAdults = tr.insertCell(4);
        tdForAdults.innerText = item.isForAdults ? 'Yes' : 'No';
        let tdEdit = tr.insertCell(5);
        tdEdit.innerHTML = `<button onclick="displayEditForm(${item.id})">edit</button>`;
        let tdDelete = tr.insertCell(6);
        tdDelete.innerHTML = `<button onclick="deleteItem(${item.id})">delete</button>`;
    });
    books = data; // Update the books array
}