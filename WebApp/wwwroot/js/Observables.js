export class ObservableSet extends Set {
    constructor() {
        super();
        this.callbacks = {
            "add": new Set(),
            "added": new Set(),
            "delete": new Set(),
            "deleted": new Set(),
        };
    }
    addEventListener(event, callback) {
        this.callbacks[event].add(callback);
    }
    emit(event, value) {
        for (const callback of this.callbacks[event]) {
            callback(value);
        }
    }
    add(value) {
        this.emit("add", value);
        const result = super.add(value);
        this.emit("added", value);
        return result;
    }
    delete(value) {
        this.emit("delete", value);
        const result = super.delete(value);
        this.emit("deleted", value);
        return result;
    }
}
