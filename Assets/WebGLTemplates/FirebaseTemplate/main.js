// Initialize Firebase

/***
  add your own firebase details here
***/
var config = {
  apiKey: "",
  authDomain: "",
  databaseURL: "",
  projectId: "",
  storageBucket: "",
  messagingSenderId: ""
};
firebase.initializeApp(config);

var database = firebase.database();
var gameInstance = null;
var loginContainer = document.getElementById('login-container');
var playButton = document.getElementById('play-button');
var gameContainer = document.getElementById('game-container');
var fullscreenPromptContainer = document.getElementById('fullscreen-prompt-container');
var fullscreenButton = document.getElementById('fullscreen-button');
var logoutButton = document.getElementById('logout-button');
var fullscreenYesButton = document.getElementById('fullscreen-yes-button');
var fullscreenNoButton = document.getElementById('fullscreen-no-button');
var closeOnceNoticeButton = document.getElementById('close-once-notice-button');
var progressBarContainer = document.getElementById('progress-bar-container');
var progressBar = document.getElementById('progress-bar');
var footer = document.getElementById('footer');
var progress = 0;
var uid = null;
var email = null;
var nPendingSaves = 0;
var isConnectedRef = database.ref('/.info/connected');
var isConnected = false;

// Warn if the player tries to leave with with pending saves.
window.addEventListener('beforeunload', function(e) {
  if (nPendingSaves > 0) {
    e.preventDefault();
    e.returnValue = '';
  }
});

isConnectedRef.on('value', function(status) {
  isConnected = status.val() === true;

  if (isConnected) {
    console.log("[Firebase] connected");
  } else {
    console.log("[Firebase] lost connection");
  }

  if (gameInstance != null) {
    sendConnectionStatus();
  }
});

function sendConnectionStatus() {
  gameInstance.SendMessage('Session', 'OnConnectionChange', isConnected ? 1 : 0);
}

function setProgress(proportion) {
  progressBar.style.width = Math.round(proportion * 100) + '%';
  progress = proportion;
}

fullscreenButton.onclick = function() {
  gameInstance.SetFullscreen(1);
}

logoutButton.onclick = function() {
  firebase.auth().signOut()
    .then(function() {
      location.reload();
    })
    .catch(function(error) {
      console.log("error:", error);
    }); 
};

function toInGame() {
  gameContainer.style.visibility = 'visible';
  fullscreenPromptContainer.style.display = 'none';
  footer.style.display = 'block';
  gameInstance.SendMessage('Session', 'OnVisible');
  sendConnectionStatus();
}

fullscreenYesButton.onclick = function() {
  gameInstance.SetFullscreen(1);
  toInGame();
};

fullscreenNoButton.onclick = function() {
  toInGame();
};

function onLogin() {
  uid = firebase.auth().currentUser.uid;
  email = firebase.auth().currentUser.email;
  progressBarContainer.style.display = 'block';
  loginContainer.style.display = 'none';
  gameInstance = UnityLoader.instantiate('game-container', buildUrl, {
    onProgress: function(gameInstance, proportion) {
      setProgress(proportion);
    },
    Module: {
      noInitialRun: false,
      onRuntimeInitialized: function() {
        fullscreenPromptContainer.style.display = 'block';
        progressBarContainer.style.display = 'none';
      }
    }
  });
}

var uiConfig = {
  callbacks: {
    signInSuccessWithAuthResult: function(authResult, redirectUrl) {
      return false;
    },
    uiShown: function() {
    }
  },
  signInOptions: [
    {
      provider: firebase.auth.GoogleAuthProvider.PROVIDER_ID,
      customParameters: {
        prompt: 'select_account'
      },
    }
  ],
  tosUrl: 'http://www.snowdaylearninglab.org/Beats_Empire_TOU.html',
  privacyPolicyUrl: 'https://www.tc.columbia.edu/policylibrary/general-counsel-/privacy-policy'
};

firebase.auth().onAuthStateChanged(function(user) {
  if (user) {
    onLogin();
  } else {
    var ui = new firebaseui.auth.AuthUI(firebase.auth());
    ui.start('#login-container', uiConfig);
  }
});

setProgress(0);
