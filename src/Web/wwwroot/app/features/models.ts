export enum FeatureStatus {
    Enabled, Disabled
}

export class Feature {

    constructor(public name: string, public status: FeatureStatus) {
    }
}

export class FeatureSet {
    constructor(public featuresToEnable: string[], public featuresToDisable: string[]) {}
}