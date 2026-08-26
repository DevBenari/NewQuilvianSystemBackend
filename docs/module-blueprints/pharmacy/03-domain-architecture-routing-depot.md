# Farmasi — Arsitektur Domain Routing Depo

| Field | Value |
| --- | --- |
| Architecture ID | `PHA-DA-001` |
| Revision | `1` |
| Blueprint | `PHA-BP-001` revision `2` |
| Scope | Menentukan tepat satu Depo pelayanan untuk sebuah resep berdasarkan encounter |
| Requirement readiness | `READY_FOR_DOMAIN_DESIGN` pada `PHA-RCG-001` |
| Architecture readiness | `DOMAIN_ARCHITECTURE_READY` untuk routing Depo |
| Related decisions | `PHA-DEC-040`, `PHA-DEC-041` |
| Related contract | `PHA-DEPOT-ROUTING-v1` |
| Backend SHA | `767470f742bc6f2eebadbd653a873f69d6f93121` |
| Baseline reference | Tidak digunakan |

## 1. Scope dan batas

Arsitektur ini hanya menentukan Depo pelayanan. Hasil routing tidak mereservasi stok, tidak mengurangi stok, tidak mengubah pembayaran, dan tidak mengganti keputusan klinis dokter.

Slice reservasi serta penyerahan tetap terblokir oleh `PHA-OQ-009` dan `PHA-OQ-014` sampai `PHA-OQ-017`.

## 2. Istilah domain

| Istilah | Makna tunggal |
| --- | --- |
| Encounter | Episode layanan pasien yang menjadi sumber jenis layanan, unit pelayanan, dan klinik |
| Depo pelayanan | Lokasi Farmasi yang sah untuk melakukan dispensing bagi encounter tertentu |
| Kandidat Depo | Lokasi yang lolos syarat aktif, Farmasi, boleh dispensing, bukan Gudang Utama, dan bukan karantina |
| Routing Depo | Keputusan deterministik untuk memilih tepat satu kandidat berdasarkan encounter |
| Ambiguous routing | Kondisi ketika lebih dari satu kandidat memiliki prioritas yang sama sehingga sistem wajib menolak |
| Unresolved routing | Kondisi ketika tidak ada kandidat yang memenuhi aturan sehingga sistem wajib menolak |

## 3. Peta bounded context

| Context ID | Bounded context | Tanggung jawab | Ownership | Hubungan |
| --- | --- | --- | --- | --- |
| `PHA-CTX-REG` | Registration/Encounter | Memiliki identitas dan klasifikasi layanan encounter | Existing, authoritative upstream | Menyediakan `EncounterType`, `ServiceUnitId`, dan `ClinicId` |
| `PHA-CTX-MD` | Health Services Master Data | Memiliki konfigurasi lokasi penyimpanan/Depo | Existing, authoritative upstream | Menyediakan atribut kandidat lokasi |
| `PHA-CTX-PHR` | Pharmacy Prescription Fulfillment | Meminta dan memakai hasil routing untuk pelayanan resep | Extend, downstream consumer | Memiliki aturan pemilihan, tetapi tidak mengambil alih master encounter/lokasi |

Registration dan Master Data tidak bergantung kepada Pharmacy. Pharmacy bergantung secara baca kepada keduanya melalui adapter routing.

## 4. Katalog konsep domain

| Concept ID | Nama | Klasifikasi | Pemilik | Identitas | Ownership | Invariant utama | Bukti |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `PHA-CONCEPT-001` | Encounter Reference | `REFERENCE_DATA` | Registration | `EncounterId` | `Existing` | Referensi harus menunjuk encounter aktif yang memiliki layanan | `TrxPatientEncounter` pada `Areas/HealthServices/RegistrationManagement/Models/TrxPatientEncounter.cs` @ SHA backend |
| `PHA-CONCEPT-002` | Pharmacy Location Reference | `REFERENCE_DATA` | Master Data | `StorageLocationId` | `Existing` | Pharmacy tidak membuat sumber kebenaran lokasi kedua | `MstDrugStorageLocation` pada `Areas/HealthServices/MasterData/Models/MstDrugStorageLocation.cs:8` @ SHA backend |
| `PHA-CONCEPT-003` | Depot Routing Criteria | `VALUE_OBJECT` | Pharmacy | Kombinasi jenis layanan, `ServiceUnitId`, dan `ClinicId` | `Adapter/View` | Nilai berasal dari satu snapshot encounter dan tidak boleh dicampur dengan encounter lain | `PHA-DEPOT-ROUTING-v1` |
| `PHA-CONCEPT-004` | Depot Routing Result | `VALUE_OBJECT` | Pharmacy | Encounter, lokasi terpilih, rule version | `New` secara logis | Hasil sukses memiliki tepat satu lokasi; hasil gagal tidak memiliki lokasi terpilih | `PHA-DEC-040` |
| `PHA-CONCEPT-005` | Depot Routing Resolved | `DOMAIN_EVENT` | Pharmacy | Correlation dan encounter | `New` secara logis | Diterbitkan hanya setelah hasil tepat satu | `PHA-GAP-ROUTE-001` sebagai audit non-blocking |
| `PHA-CONCEPT-006` | Depot Routing Rejected | `DOMAIN_EVENT` | Pharmacy | Correlation dan encounter | `New` secara logis | Menyimpan alasan `NoCandidate` atau `AmbiguousCandidate`, tanpa memilih lokasi | `PHA-DEPOT-ROUTING-v1` |

Konsep `New` di atas adalah konsep domain logis. Dokumen ini tidak memutuskan bahwa semuanya harus menjadi tabel database.

## 5. Model aggregate dan layanan domain

Routing Depo tidak memerlukan aggregate root baru. Ia adalah layanan domain deterministik pada batas Pharmacy yang membaca dua sumber authoritative:

1. `Encounter Reference` dari Registration.
2. Daftar `Pharmacy Location Reference` dari Master Data.
3. Resolver membentuk `Depot Routing Criteria`.
4. Resolver menyaring kandidat dan menerapkan prioritas `PHA-DEPOT-ROUTING-v1`.
5. Resolver menghasilkan `Depot Routing Result` sukses atau gagal.

Invariant resolver:

- kandidat wajib aktif dan tidak dihapus;
- kandidat wajib `IsPharmacyLocation = true` dan `IsAllowDispensing = true`;
- kandidat dengan `IsMainWarehouse = true` atau `IsQuarantineLocation = true` dilarang;
- satu level prioritas hanya sah bila menghasilkan tepat satu kandidat;
- resolver tidak boleh memilih kandidat pertama saat hasil lebih dari satu;
- hasil routing tidak boleh mengubah encounter maupun lokasi master.

**Contoh:** encounter Rawat Jalan memiliki `ClinicId = Klinik Anak`. Dua lokasi tersedia, tetapi hanya “Depo Klinik Anak” memiliki `ClinicId` yang sama. Resolver menghasilkan satu lokasi tersebut. Jika ada dua Depo dengan `ClinicId` sama, hasilnya `AmbiguousCandidate`, bukan memilih berdasarkan urutan database.

## 6. Model relasi

| Sumber | Tujuan | Kardinalitas | Makna | Lifecycle/ownership |
| --- | --- | --- | --- | --- |
| Encounter | Depot Routing Result | `1 : 0..1` per evaluasi | Satu evaluasi dapat berhasil memilih satu Depo atau gagal tanpa Depo | Encounter tetap dimiliki Registration |
| Service Unit | Storage Location | `1 : 0..n` | Unit dapat dikonfigurasi ke beberapa lokasi, tetapi resolver menuntut tepat satu pada prioritas aktif | Konfigurasi dimiliki Master Data |
| Clinic | Storage Location | `1 : 0..n` | Klinik dapat menjadi scope Depo Rawat Jalan | Konfigurasi dimiliki Master Data |
| Prescription | Depot Routing Result | `1 : 0..n` evaluasi, satu hasil aktif saat proses | Resep boleh dievaluasi ulang sebelum reservasi | Resep dimiliki Pharmacy; hasil lama tetap dapat diaudit |

Penghapusan encounter atau lokasi tidak dilakukan oleh routing. Lokasi yang dinonaktifkan keluar dari evaluasi berikutnya tanpa menghapus histori hasil sebelumnya.

## 7. Lifecycle routing

| Dari | Tindakan | Ke | Pelaku | Syarat |
| --- | --- | --- | --- | --- |
| `NotEvaluated` | Evaluasi encounter dan kandidat | `Resolved` | Sistem atas proses Pharmacy | Tepat satu kandidat pada prioritas yang berlaku |
| `NotEvaluated` | Evaluasi tanpa kandidat | `RejectedNoCandidate` | Sistem | Tidak ada kandidat sah |
| `NotEvaluated` | Evaluasi dengan kandidat ganda | `RejectedAmbiguous` | Sistem | Lebih dari satu kandidat pada prioritas yang sama |
| `Resolved` | Validasi ulang sebelum reservasi | `Resolved` dengan evaluasi baru | Sistem | Lokasi lama masih sah dan tetap terpilih |
| `Resolved` | Validasi ulang menemukan perubahan | `RejectedNoCandidate` atau `RejectedAmbiguous` | Sistem | Konfigurasi berubah; jangan memindahkan Depo diam-diam |

`Resolved` bukan status pembayaran dan bukan status stok. Koreksi dilakukan dengan evaluasi baru serta audit hasil lama, bukan mengubah histori lama.

## 8. Authorization

- Sistem boleh menjalankan resolver sebagai bagian dari proses resep.
- Pengguna yang dapat membaca resep hanya melihat Depo sesuai hak akses data yang berlaku.
- Wewenang mengubah master lokasi tetap milik Master Data dan berada di luar slice ini.
- Resolver tidak memberi pengguna Farmasi hak untuk mengubah encounter, klinik, unit, atau atribut lokasi.

Tidak ada peran baru yang dikarang oleh arsitektur ini.

## 9. Audit dan histori

Setiap evaluasi material perlu dapat ditelusuri melalui:

- encounter dan prescription correlation;
- waktu evaluasi;
- versi aturan `PHA-DEPOT-ROUTING-v1`;
- input jenis layanan, unit, dan klinik;
- kandidat yang memenuhi filter pada prioritas yang dievaluasi;
- lokasi terpilih atau alasan penolakan;
- sumber proses yang meminta evaluasi.

**Contoh:** audit menunjukkan evaluasi pukul 09.10 memakai Klinik Anak dan memilih Depo Anak. Validasi ulang pukul 09.20 gagal karena Depo dinonaktifkan. Histori pukul 09.10 tidak ditimpa.

## 10. Integrasi, concurrency, dan idempotency

| Produsen | Konsumen | Data/tujuan | Arah | Kegagalan |
| --- | --- | --- | --- | --- |
| Registration | Pharmacy resolver | Snapshot encounter | Sinkron baca internal | Encounter tidak ada/tidak valid menghasilkan penolakan |
| Master Data | Pharmacy resolver | Kandidat lokasi aktif | Sinkron baca internal | Nol atau ganda menghasilkan penolakan konfigurasi |
| Pharmacy resolver | Prescription fulfillment | Hasil routing | Internal | Proses berhenti sebelum reservasi bila gagal |

Permintaan evaluasi dengan encounter, prescription, dan versi aturan yang sama harus memberi hasil konsisten selama konfigurasi sumber tidak berubah. Setiap proses reservasi wajib memvalidasi routing ulang agar perubahan lokasi tidak menyebabkan stok diambil dari Depo yang sudah tidak sah.

Rekonsiliasi konfigurasi dilakukan oleh owner Master Data; resolver hanya melaporkan kegagalan dan tidak memperbaiki master secara otomatis.

## 11. Dampak Billing

Klasifikasi: tidak ada dampak charge langsung. Routing tidak membuat, mengubah, membatalkan, atau membalik tagihan. Dependency Billing pada reservasi tetap belum terselesaikan dan berada di luar scope arsitektur ini.

## 12. Dampak keselamatan klinis

Klasifikasi: relevan terhadap keselamatan operasional obat. Salah Depo dapat menyebabkan ketersediaan obat dinilai dari lokasi yang keliru. Invariant tepat satu hasil dan larangan fallback acak mencegah pemrosesan berlanjut dalam kondisi ambigu.

Routing tidak menilai kesesuaian klinis obat, alergi, dosis, atau interaksi.

## 13. Keadaan saat ini dan target

| Area | Keadaan source saat ini | Target disetujui |
| --- | --- | --- |
| Encounter | Memiliki `EncounterType`, `ServiceUnitId`, dan `ClinicId` | Dipakai ulang, tidak diduplikasi |
| Lokasi | Memiliki scope unit/klinik dan flag Farmasi, dispensing, gudang utama, karantina | Dipakai ulang melalui adapter resolver |
| Resolver | Belum terbukti tersedia | Tambahkan tanggung jawab domain deterministik pada Pharmacy |
| Reservasi | Ledger/reservasi authoritative belum tersedia | Tidak termasuk implementasi routing |

## 14. Gap dan blocker

- Tidak ada blocker bisnis untuk arsitektur routing Depo.
- `PHA-GAP-ROUTE-001` mengenai bentuk penyimpanan audit tetap menjadi keputusan desain implementasi non-blocking.
- `PHA-OQ-009` dan `PHA-OQ-014` sampai `PHA-OQ-017` tetap memblokir slice lain dan tidak diselesaikan oleh dokumen ini.

## 15. Traceability

| Requirement/decision | Konsep/domain result |
| --- | --- |
| `PHA-DEC-040` | Resolver deterministik, filter kandidat, tepat satu hasil, rejection |
| `PHA-DEC-041` | Hasil routing tidak mengambil stok; validasi ulang sebelum reservasi |
| `PHA-DEPOT-ROUTING-v1` | Prioritas Rawat Jalan, IGD, Rawat Inap dan jalur exception |
| `PHA-DEP-004` | Encounter dan lokasi existing digunakan melalui adapter |
| `PHA-RCG-001` | Scope routing berdiri sendiri dari reservasi dan penyerahan yang terblokir |

## 16. Kesiapan dan handoff

`PHA-DA-001` berstatus `DOMAIN_ARCHITECTURE_READY` hanya untuk routing Depo. Slice ini boleh diteruskan ke `design-business-module` untuk menyusun kontrak implementasi target. Reservasi dan penyerahan tidak termasuk dalam handoff tersebut.

