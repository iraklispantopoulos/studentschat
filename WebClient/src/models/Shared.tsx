export class Speech {
    audioUrl: string;
    mouthCuesUrl: string;
    speechFileName: string;

    constructor(audioUrl: string, mouthCuesUrl:string,speechFileName:string) {
        this.audioUrl = audioUrl;
        this.mouthCuesUrl = mouthCuesUrl;
        this.speechFileName = speechFileName;
    }
}