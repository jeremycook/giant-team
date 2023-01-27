export default class Exception {
    data: any[];
    constructor(public source: any, public message: string, ...data: any[]) {
        this.data = data;
    }
}