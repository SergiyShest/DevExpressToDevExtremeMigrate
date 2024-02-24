

export const baseMixin = {

	data: () => ({
		loadingData: false,
		mode:'edit'
     })
,
	methods: {
		GenerateGuid() { // Public Domain/MIT
			var d = new Date().getTime();//Timestamp
			var d2 = ((typeof performance !== 'undefined') && performance.now && (performance.now() * 1000)) || 0;//Time in microseconds since page-load or 0 if unsupported
			return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
				var r = Math.random() * 16;//random number between 0 and 16
				if (d > 0) {//Use timestamp until depleted
					r = (d + r) % 16 | 0;
					d = Math.floor(d / 16);
				} else {//Use microseconds since page-load if supported
					r = (d2 + r) % 16 | 0;
					d2 = Math.floor(d2 / 16);
				}
				return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
			});
		},

		ExtractItem(val)
		{
			if (val.Errors && val.Errors.length > 0) {
				this.ShowErrors(val);
			  } else {
				if(val.Item)
				return val.Item;
			  }
		},

		ShowErrors(item) {
			this.PlayError()
			var stringError;
			console.log(item.Errors);
			if(Array.isArray(item.Errors)){
			item.Errors.forEach(x => {
				if (x.Name) {
					stringError += x.Name + ":";
				}
				stringError += x.Errors.join("\r\n");

			});
		} else stringError=item.Errors;
			if (stringError)
				this.ShowAlert(stringError);
		}
		,
		PlayOk() {
			this.PlaySound("/Content/Sounds/code-found.mp3")
		}
		,
		PlayError() {
			this.PlaySound("/Content/Sounds/bad-beep.mp3")
		}
		,
		PlaySound(sound) {
			var path = document.location.origin + sound;//;
			var audio = new Audio(path);
			audio.autoplay = true;
		}
		,
		CloseThis: function () {
			window.parent.postMessage("CloseUrlForm", '*'); //send message parent window for  close this form

		}
		,
		ShowAlert(message) {
			console.log(message);
			alert(message)
		}
		,
		FetchJson(pathEnd, execFunction, data) {
			this.fetch(execFunction, pathEnd, data)
		}
		,
		//универсальная функция получения/отправки данных
		fetch: function (execFunction, pathEnd, data = null) {
			if (pathEnd.startsWith("/")) { pathEnd = pathEnd.substring(1); }
			this.loadingData = true;
			const fetchRef = execFunction;
			const path = document.location.origin  +'/'+ pathEnd
			//const path ='http://localhost:5056/'+ pathEnd
			const json = JSON.stringify(data);
			fetch(path,
				{
					method: 'POST', // *GET, POST, PUT, DELETE, etc.
					mode: 'no-cors', // no-cors, *cors, same-origin
					body: json, //sending data
					referrer: 'unsafe-url',
					headers: {
						'Content-Type': 'application/json'
					}
				})
				.then((response) => {
					this.loadingData = false;
					if (response )
					{
						return response.json();
					}  
				})
				.then((retData) => {
					if(retData){
					{console.log(retData)	
					fetchRef(retData)
				}
			}
					this.loadingData = false;
	
				}).catch(error => {
					this.loadingData = false;
					console.error("Error while getting server data: " + path + ' :' + error)
					//this.ShowAlert("Error while getting server data: " + path + ' :' + error);
				});

		}
        ,
        GetDate(dateStr){
           if(dateStr&&dateStr.length>10){
			return dateStr.substring(0,10)
		   }
		   return dateStr;
		},
        GetAge(dateStr){
			var dob = new Date(dateStr);
			if(dateStr==null || dateStr=='') {
			  
			  return null; 
			} else {
			var month_diff = Date.now() - dob.getTime();
			var age_dt = new Date(month_diff); 
			var year = age_dt.getUTCFullYear();
			var age = Math.abs(year - 1970);
			return age;
		}

		}
	}
}
