# Educational references

The field guide separates attack symptoms, the modeled defense, and real-world limitations. The game runs local simulations; no vehicle, navigation service or network is attacked.

- **Replay protection:** Authenticating a message does not establish freshness. The T defense represents rejecting reused commands with authenticated sequence/timeliness data and nonces. [NIST SP 800-38B, Appendix C](https://nvlpubs.nist.gov/nistpubs/Legacy/SP/nistspecialpublication800-38b.pdf) describes sequence numbers, timestamps and nonces for replay protection. [NIST's replay-attack definition](https://csrc.nist.gov/glossary/term/replay_attack) explains reuse of captured messages.
- **Firmware recovery:** The Y action represents validating the update source and recovering a trusted firmware image. [NIST SP 800-193](https://csrc.nist.gov/pubs/sp/800/193/final) organizes firmware resilience around protection, detection and recovery. Signed firmware still needs trusted keys and an appropriate recovery policy; a signature is not a guarantee of bug-free software.
- **Ransomware recovery:** The F action models restoring a verified offline backup after isolating the compromised environment. [CISA's StopRansomware Guide](https://www.cisa.gov/stopransomware/ransomware-guide) recommends offline backups and tested restoration.
- GPS-route verification is a simplified scenario: an authenticated route cannot by itself validate a raw satellite position. The guide explicitly explains independent sensors and signal checks.
- Offline navigation preserves availability but does not end a denial-of-service attack.
- Controller isolation requires suitable gateways and safe degraded operation; it is not a universal switch on existing vehicles.
- V2X authentication and duplicate-identity rejection model only part of Sybil resistance. Certificate lifecycle, plausibility checking and privacy-aware identity management also matter.
- Road collisions are consequences of malicious behavior, rather than examples of a cyber attack by themselves.

Sources checked 2026-09-10. These principles inform the educational examples; the arcade outcomes are fictional simplifications.

## Lighting takeover

The night scenario applies authentication, authorization, remote-control isolation and local fallback to road-lighting infrastructure. It is a fictional simplified example of operational technology security. [NIST SP 800-82 Rev. 3](https://csrc.nist.gov/pubs/sp/800/82/r3/final) discusses securing systems that directly affect the physical environment while preserving safety and reliability. Headlights remain independent in the game.
