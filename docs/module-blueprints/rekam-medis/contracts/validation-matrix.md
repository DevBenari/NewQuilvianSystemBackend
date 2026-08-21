# Validation Matrix — Rekam Medis Existing Clinical Foundation

| Field | Nilai |
| --- | --- |
| `contract_version` | `rm-existing-clinical-validation-v0.2-draft` |
| Status | `draft` |
| Owner | Clinical Management dan owner reference; Unit RM untuk interpretasi kelengkapan |
| Approval | Belum tersedia — `RM-APR-002` |
| Traceability | `RM-SIG-*`, `RM-COR-*`, `RM-CLS-*`, `RM-REL-*`, `RM-INT-*` |

| Aturan | Berlaku pada | Kondisi | Pesan bagi pengguna | Kode |
| --- | --- | --- | --- | --- |
| Patient wajib ada | Semua create/read klinis | `PatientId` tidak ditemukan | “Data pasien tidak ditemukan pada sumber resmi.” | `404` |
| Encounter wajib ada bila diminta | Fakta per episode | `EncounterId` tidak ditemukan | “Episode pelayanan tidak ditemukan.” | `404` |
| Patient–encounter harus cocok | Semua fakta dengan encounter | `Encounter.PatientId` berbeda | “Episode pelayanan tidak sesuai dengan pasien yang dipilih.” | `409` |
| Konsultasi harus milik encounter/patient yang sama | Diagnosis, tindakan, CPPT, dokumen, consent | Reference berbeda konteks | “Konsultasi tidak sesuai dengan pasien atau episode pelayanan.” | `409` |
| Assessment harus sesuai konteks | Konsultasi/CPPT/vital | Assessment berasal dari episode lain | “Assessment tidak sesuai dengan episode pelayanan.” | `409` |
| Hanya satu diagnosis utama aktif | Set primary/finalisasi | Lebih dari satu `IsPrimary=true` | “Diagnosis utama harus tepat satu.” | `409` |
| Diagnosis utama wajib sebelum finalisasi rawat jalan | Finalization validation | Tidak ada diagnosis utama | “Diagnosis utama belum ditetapkan.” | `400` |
| SOAP wajib untuk kelengkapan rawat jalan | Konsultasi complete | Bagian wajib SOAP kosong | “Catatan SOAP belum lengkap.” | `400` |
| Assessment awal wajib | Kelengkapan rawat jalan | Assessment tidak completed/bertanda tangan | “Asesmen awal belum lengkap atau belum ditandatangani.” | `400` target RM |
| Tindakan conditional | Checklist | Event tindakan nyata belum terjadi | Jangan membuat item wajib tindakan. | Domain validation |
| Pembatalan pemicu tidak menghapus item otomatis | Checklist | Event yang sudah memicu item dibatalkan | “Item wajib memerlukan review sebelum dikeluarkan.” | `409` target RM |
| Record final tidak boleh diubah | PUT SOAP/CPPT/dokumen/consent/fakta resmi | Bukti signature/finality sudah ada | “Catatan telah ditandatangani. Buat koreksi atau addendum.” | `409` target RM |
| Record final tidak boleh dihapus | DELETE/cancel | Record sudah signed/final | “Catatan resmi tidak dapat dihapus.” | `409` target RM |
| Completion existing bukan signature RM | Completeness mapping | Hanya `Completed/Verified/Approved/Signed` owner tersedia | Tandai bukti signature RM belum tersedia. | Domain validation |
| Signature memerlukan reauth dan content hash | Pengesahan target RM | Reauth, signer role, meaning, atau hash tidak ada | “Tanda tangan belum dapat disimpan karena bukti pengesahan tidak lengkap.” | `400` target RM |
| Permission generic belum cukup | Semua baca/tulis klinis | Role ada tetapi assignment aktif tidak ada | “Anda tidak memiliki hubungan pelayanan aktif dengan pasien ini.” | `403` target RM |
| CPPT generated read-only tidak boleh diubah | PUT CPPT | `IsReadOnlyGenerated=true` | “Catatan yang dibuat dari sumber lain tidak dapat diubah di sini.” | `400/409` existing |
| Alergi aktif tidak boleh hilang tanpa review | Cancel/delete allergy | Alergi aktif/high risk | “Perubahan alergi memerlukan alasan dan peninjauan keselamatan.” | `409` target extension |
| Nilai vital kritis perlu acknowledgement owner | Notify/complete | Alert kritis belum ditindaklanjuti | “Tanda vital kritis belum memiliki bukti tindak lanjut.” | `400` target extension |
| Consent signed tidak boleh diubah langsung | PUT/delete consent | `ConsentStatus` ≥ `Signed` | “Consent yang sudah ditandatangani harus dikoreksi melalui alur resmi.” | `409` target extension |
| File hash wajib untuk bukti versi | Dokumen yang diperhitungkan RM | `FileHash` kosong | “Bukti versi dokumen belum tersedia.” | `400` target extension |
| Owner unavailable bukan empty | Lookup/list | Timeout/gagal | “Sumber data belum dapat diverifikasi. Coba lagi.” | `503` |
| Duplicate submit harus idempotent | Mutation resmi | Key dan payload sama | Kembalikan hasil pertama; jangan buat record kedua. | `200` replay target |
| Kunci sama, isi berbeda | Mutation resmi | Idempotency key sama, hash berbeda | “Permintaan yang sama membawa isi berbeda.” | `409` target |
| Break-glass/release tetap nonaktif | UI/API berisiko | Policy/approval belum lengkap | “Fitur belum dapat digunakan karena policy belum disahkan.” | `403/503` fail-closed |

**Contoh 1:** patient A dipilih tetapi konsultasi menunjuk encounter milik patient B. Sistem
menolak `409` dan tidak membuka SOAP.

**Contoh 2:** dokter menekan complete dua kali. Permintaan kedua dengan key dan isi sama harus
mengembalikan hasil pertama. Jika SOAP berubah tetapi key tetap sama, sistem menolak `409`.
