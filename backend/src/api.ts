import { type NextFunction, type Request, type Response } from "express";
const Openfort = require('@openfort/openfort-node').default;

const policy_id = 'pol_0b74cbac-146b-4a1e-98e1-66e83aef5deb';
const contract_id = 'con_42883506-04d5-408e-93da-2151e293a82b';
const chainId = 80001;

export class MintController {

    constructor() {
        this.run = this.run.bind(this);
    }

    async run(req: Request, res: Response, next: NextFunction) {

        const openfort = new Openfort(process.env.OPENFORT_SECRET_KEY);
        const accessToken = req.headers.authorization?.split(' ')[1];

        if (!accessToken) {
            return res.status(401).send({
              error: 'You must be signed in to view the protected content on this page.',
            });
          }

        const response = await openfort.iam.verifyOAuthToken({
            provider: 'firebase',
            token: accessToken,
            tokenType: 'idToken',
          });
          
        if (!response?.id) {
        return res.status(401).send({
            error: 'Invalid token or unable to verify user.',
        });
        }


        const playerId = response.id;

        const interaction_mint = {
            contract: contract_id,
            functionName: "mint",
            functionArgs: [playerId],
        };

        try {
            const transactionIntent = await openfort.transactionIntents.create({
                player: playerId,
                policy: policy_id,
                chainId,
                interactions: [interaction_mint],
            });

            console.log("transactionIntent", transactionIntent)
            return res.send({
                data: transactionIntent,
            });
        } catch (e: any) {
            console.log(e);
            return res.send({
                data: null,
            });
        }

    }
}
