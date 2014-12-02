// See Program.fsx for information on how this is used
declare class ImprovedNoise {
   constructor();

   noise(x: number, y: number, z: number):number;
};

declare module THREE {
   interface Camera {}

   export class FirstPersonControls {
      movementSpeed: number;
      lookSpeed: number;
      constructor(camera:Camera);

      handleResize();
      update(delta:number);
   }
}