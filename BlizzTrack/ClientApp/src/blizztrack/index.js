export class BlizzTrack {
    static async Call(func) {
        return await fetch(`/api/${func}`).then(res => res.json());
    }
}