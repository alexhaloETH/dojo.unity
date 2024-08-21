mergeInto(LibraryManager.library, {
    //need the copy clipboard

    CopyToClipboardImpl: function(textPointer) {
      var text = UTF8ToString(textPointer);
      navigator.clipboard.writeText(text).then(function() {
        console.log('Async: Copying to clipboard was successful!');
      }, function(err) {
        console.error('Async: Could not copy text: ', err);
      });
    },

    RefreshPage: function() {
      window.location.reload();
    },

    OpenURL: function(urlPtr) {
      var url = UTF8ToString(urlPtr); 
      window.open(url, '_blank').focus();
    },

    // Local Storage Functions, Shouldnt really use this aas the prefs should be enough but we can for now have this just in case
    SaveToLocalStorage: function (key, value) {
      key = UTF8ToString(key);
      value = UTF8ToString(value);
      localStorage.setItem(key, value);
    },

    getAllData: function () {
      let items = [];
      for (let i = 0; i < localStorage.length; i++) {
        const key = localStorage.key(i);
        const value = localStorage.getItem(key);
        items.push({ [key]: value });
      }

      var createdJson = JSON.stringify(items);

      return createdJson;
    },

    ClearAllLocalData: function () {
      localStorage.clear();
    },

    PoseidonHash: function(feltArrayJsonPtr, length) {
        var feltArray = new Array(length);

        for (var i = 0; i < length; i++) {
            feltArray[i] = UTF8ToString(HEAP32[(feltArrayJsonPtr + (i * 4)) >> 2]);
        }

        const starkPtr = window.starkAdditional;

        const arrOfFelts = feltArray.map(starkPtr.cairo.felt);
        const pedersenHash = starkPtr.hash.computePoseidonHashOnElements(arrOfFelts.map(BigInt));
        const feltOfPedersenHash = starkPtr.cairo.felt(pedersenHash);

        var bufferSize = lengthBytesUTF8(feltOfPedersenHash) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(feltOfPedersenHash, buffer, bufferSize);

        return buffer;
    }
});
