

export const baseMixin = {

	data: () => ({
		loadingData: false,
		mode:'edit'
     })
,
	methods: {
		generateGuid() { 
			let d = new Date().getTime();//Timestamp
			let d2 = ((typeof performance !== 'undefined') && performance.now && (performance.now() * 1000)) || 0;//Time in microseconds since page-load or 0 if unsupported
			return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
				let r = Math.random() * 16;//random number between 0 and 16
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

		extractItem(val)
		{
			if (val.Errors && val.Errors.length > 0) {
				this.showErrors(val);
			  } else {
				if(val.Item)
				return val.Item;
			  }
		},

		showErrors(item) {
			this.playError()
			let stringError;
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
		playOk() {
			this.playSound("/Content/Sounds/code-found.mp3")
		}
		,
		playError() {
			this.playSound("/Content/Sounds/bad-beep.mp3")
		}
		,
		playSound(sound) {
			let path = document.location.origin + sound;//;
			let audio = new Audio(path);
			audio.autoplay = true;
		}
		,
		closeClick() {
			closePopup()
		}
		,
		showAlert(message) {
			alert(message)
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
				
					fetchRef(retData)
				
			}
					this.loadingData = false;
	
				}).catch(error => {
					this.loadingData = false;
					console.error("Error while getting server data: " + path + ' :' + error)
					//this.ShowAlert("Error while getting server data: " + path + ' :' + error);
				});

		}
        ,

	}
}
