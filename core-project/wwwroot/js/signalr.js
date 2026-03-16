let connection = null;

// יצור קשר עם LibraryHub
function initializeSignalR() {
    // בדוק אם יש טוקן
    const token = sessionStorage.getItem('jwtToken');
    if (!token) {
        console.log('No token found, skipping SignalR initialization');
        return;
    }

    // יצור קשר עם הרי-ל hub
    connection = new signalR.HubConnectionBuilder()
        .withUrl("/libraryHub", {
            accessTokenFactory: () => token // שלח את הטוקן לאימות
        })
        .withAutomaticReconnect()
        .build();

    // טפל בהתחברות
    connection.on("BookAdded", function (book) {
        console.log("ספר חדש נוסף:", book);
        // רענן את רשימת הספרים
        if (typeof getItems === 'function') {
            getItems();
        }
        // הצג התראה
        showNotification(`ספר חדש נוסף: ${book.Name}`);
    });

    // טפל בעדכון
    connection.on("BookUpdated", function (book) {
        console.log("ספר עודכן:", book);
        // רענן את רשימת הספרים
        if (typeof getItems === 'function') {
            getItems();
        }
        // הצג התראה
        showNotification(`ספר עודכן: ${book.Name}`);
    });

    // טפל במחיקה
    connection.on("BookDeleted", function (bookId) {
        console.log("ספר נמחק בעל ID:", bookId);
        // רענן את רשימת הספרים
        if (typeof getItems === 'function') {
            getItems();
        }
        // הצג התראה
        showNotification(`ספר בעל ID ${bookId} נמחק`);
    });

    // התחבר
    connection.start()
        .then(() => console.log("✅ Connected to LibraryHub"))
        .catch(error => {
            console.error("❌ Error connecting to LibraryHub:", error);
            // נסה להתחבר מחדש אחרי שנייה
            setTimeout(() => initializeSignalR(), 1000);
        });

    // טפל בנתוק
    connection.onclose(() => {
        console.log("🔌 Disconnected from LibraryHub");
    });

    // טפל בשגיאות
    connection.onreconnected(() => {
        console.log("🔄 Reconnected to LibraryHub");
    });

    connection.onreconnecting(() => {
        console.log("⚠️ Attempting to reconnect to LibraryHub...");
    });
}

// הצג התראה למשתמש
function showNotification(message) {
    // בדוק אם יש כבר התראה בדף
    let notificationContainer = document.getElementById('notification-container');
    if (!notificationContainer) {
        notificationContainer = document.createElement('div');
        notificationContainer.id = 'notification-container';
        notificationContainer.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            z-index: 9999;
            max-width: 400px;
        `;
        document.body.appendChild(notificationContainer);
    }

    // יצור התראה חדשה
    const notification = document.createElement('div');
    notification.style.cssText = `
        background: #4CAF50;
        color: white;
        padding: 16px;
        border-radius: 4px;
        margin-bottom: 10px;
        box-shadow: 0 4px 6px rgba(0,0,0,0.1);
        animation: slideIn 0.3s ease-in-out;
        direction: rtl;
    `;
    notification.textContent = message;

    notificationContainer.appendChild(notification);

    // הסר התראה אחרי 5 שניות
    setTimeout(() => {
        notification.style.animation = 'slideOut 0.3s ease-in-out';
        setTimeout(() => notification.remove(), 300);
    }, 5000);
}

// הוסף animation styles
const style = document.createElement('style');
style.textContent = `
    @keyframes slideIn {
        from {
            transform: translateX(400px);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOut {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(400px);
            opacity: 0;
        }
    }
`;
document.head.appendChild(style);

// אתחול כשדף טוען
document.addEventListener('DOMContentLoaded', function() {
    initializeSignalR();
});
