mergeInto(LibraryManager.library, {
  PersistFirebase: function(json) {
    json = Pointer_stringify(json);
    var save = database.ref('users/' + uid + '/logs').push();
    save.set(JSON.parse(json)); 
  },
  SaveData: function(json){
    ++nPendingSaves;
    console.log("+nPendingSaves", nPendingSaves);
    json = Pointer_stringify(json);
    database.ref('users/' + uid + '/saves').set(JSON.parse(json)).then(function() {
      --nPendingSaves;
      console.log("-nPendingSaves", nPendingSaves);
      SendMessage('Game Controller', 'IndicateSave');
    });
  },
  LoadData: function() {
    database.ref('users/' + uid + '/saves').once('value').then(function(snapshot) {
      console.log(snapshot.val());
      SendMessage('Game Controller', 'LoadCallback', JSON.stringify(snapshot.val()));
    }, function (error) {
       console.log("Error: " + error.code);
    });
  },
  PendingSavesCount: function() {
    return nPendingSaves;
  },
  FirebaseUserID: function() {
    var bufferSize = lengthBytesUTF8(uid) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(uid, buffer, bufferSize);
    return buffer;
  },
  FirebaseUserEmail: function() {
    var bufferSize = lengthBytesUTF8(uid) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(email, buffer, bufferSize);
    return buffer;
  }
});
