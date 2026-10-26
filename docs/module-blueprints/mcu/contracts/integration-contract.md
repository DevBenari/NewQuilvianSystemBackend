# Kontrak Integrasi — Modul MCU

| Field | Nilai |
| --- | --- |
| Contract | `MCU-INT-v1` |
| contract_version | `v1` — status `approved` |
| Owner | Yoga Aji Pratama; pemilik modul mitra untuk sisi masing-masing |
| approved_by / approved_at | Yoga Aji Pratama / 2026-10-21. Sisi mitra: Lab/Rad (`MCU-COORD-002`, `008`) dan Registrasi disetujui pemiliknya 2026-10-21 (`MCU-EVD-002`); sisi Billing `INT-MCU-09` disetujui Yasmina, pemilik `billing-kasir`, 2026-10-21 (`MCU-COORD-003`) |
| input_revision | `MCU-HDA-001` bagian J; `02-backend-architecture.md` r1; decision log r5 |

Seluruh integrasi bersifat **internal** — modul dalam satu aplikasi dan satu basis data. **Tidak ada**
sistem pihak ketiga pada rilis pertama: pengiriman WhatsApp/email ditunda (`MCU-DEC-027`).

## 1. Ringkasan

| ID | Arah | Tujuan | Mekanisme | Waktu | Persetujuan mitra |
| --- | --- | --- | --- | --- | --- |
| `INT-MCU-01` | MCU → Patient Management | Cari pasien per NIK + tanggal lahir | Baca `MstPatient` (`AsNoTracking`) | Saat check-in | `MCU-COORD-009` — baca saja |
| `INT-MCU-02` | MCU → Registrasi | Buat kunjungan `MedicalCheckup` | `EncounterIntakeService.RegisterAsync` (diperluas) | Dalam transaksi check-in | Pemilik Registrasi |
| `INT-MCU-03` | MCU → Billing | Tambah baris paket | `BillingInvoiceService.AddCatalogChargeAsync` | Dalam transaksi check-in | Tidak perlu — service sudah ada |
| `INT-MCU-04` | Billing → MCU | Status lunas/terjamin | `LabPaymentClearanceRules.ReadAsync` | Saat *Mulai pemeriksaan* | Pemilik Lab (aturan milik Lab, dipakai bersama Rad) |
| `INT-MCU-05` | MCU → Laboratorium | Buat pesanan bertanda paket | `LabOrderService` (jalur MCU, diperluas) | Dalam transaksi *Mulai pemeriksaan* / tambah komponen | `MCU-COORD-002`, `008` |
| `INT-MCU-06` | MCU → Radiologi | Buat pesanan bertanda paket | `RadOrderService` (diperluas) | Sama | `MCU-COORD-002`, `008` |
| `INT-MCU-07` | Lab/Rad → MCU | Status selesai/batal | MCU membaca `LabOrder.OrderStatus`, status `RadOrder`/laporan | Saat daftar/detail dibuka dan *refresh* | — |
| `INT-MCU-08` | MCU → Lab/Rad | Batalkan pemeriksaan | Lab: `LabExaminationService.CancelAsync` per `LabExamination` (sudah ada, terbukti `MCU-EVD-002` 2.1); batal penuh: batal pesanan Lab. Rad: per pesanan | Saat batalkan komponen / batal penuh | `ARC-TECH-01` (terjawab) |
| `INT-MCU-09` | MCU → Billing | Refund sebagian/utuh | Service refund Billing **sesudah diperluas** (`MCU-DEC-043`): `SelectedBillingItemIds` + `RequestedAmount` yang benar-benar dipakai, ≤ sisa refundable baris, akumulasi per baris. Jalur hari ini mengabaikan `RequestedAmount` (`MCU-EVD-002` 2.2) | Saat batalkan komponen / batal penuh | `MCU-COORD-003` — disetujui 2026-10-21 |
| `INT-MCU-10` | MCU → Medical Fee | Sumber jasa medis | Medical Fee **membaca** `McuMedicalFeeSourceService` | Sesuai jadwal Medical Fee | `MCU-COORD-010` |
| `INT-MCU-11` | Registrasi → MCU | Status skrining selesai | MCU membaca antrean perawat kunjungan (`RegQueue`) / status kunjungan | Saat isi Pemeriksaan MCU dan approve | `MCU-COORD-004`; dibuktikan `MCU-DEC-028` |
| `INT-MCU-12` | Kiosk → MCU | Check-in / *walk-in* | Endpoint `kiosk/*` MCU; dokter MCU = dokter jaga tanggal itu (`MCU-DEC-042`) | Real-time | `MCU-COORD-006`; di luar MVP (`EPIC MCU-08`) |
| `INT-MCU-13` | RJ/RI/IGD → MCU | Rekomendasi | `POST orders/recommendations` | Real-time | Pemilik RJ/RI/IGD untuk tombolnya |

## 2. Rincian

### `INT-MCU-02` — Kunjungan Medical Check-Up

| Butir | Isi |
| --- | --- |
| Perubahan di Registrasi | `EncounterIntakeRequest` mendapat ruas jenis kunjungan dan klinik; untuk `MedicalCheckup` antrean dan skrining mengikuti master klinik MCU (`IsQueueRequired`), bukan dipaksa `false` |
| Penjaga yang tetap | `HasRegistrationAuthorityAsync` — kewenangan membuat kunjungan tetap milik Registrasi; akun admisi MCU wajib punya kewenangan itu |
| Idempotensi | `IdempotencyKey = "MCU-CHECKIN:" + McuOrder.Id`; kirim ulang mengembalikan kunjungan yang sama |
| Penjamin | Perorangan `Cash`; perusahaan `CompanyGuarantor` dengan sumber pembayaran dari `MstPatientCompanyGuarantor` |
| Gagal | Pengecualian dari Registrasi membatalkan transaksi check-in |

### `INT-MCU-03` — Baris paket

| Butir | Isi |
| --- | --- |
| Request | `AddCatalogChargeRequest { EncounterId, TariffId = AppliedTariffId, Quantity = 1, CorrelationId = McuOrder.Id, CausationId = id status history check-in }` |
| Pemilihan tarif | Pemetaan harga perusahaan yang berlaku pada **tanggal check-in**; bila tidak ada, tarif umum paket |
| Idempotensi | `CorrelationId` = `McuOrder.Id`; MCU juga menyimpan `PackageInvoiceItemId` dan tidak memanggil lagi bila sudah terisi |
| PPN & biaya administrasi | Mengikuti Billing: kena PPN (`BKC-DEC-083`); tidak ada biaya administrasi (`MCU-DEC-026`) |

### `INT-MCU-05`/`06` — Pesanan Lab/Rad bertanda paket

| Butir | Isi |
| --- | --- |
| Lab | Komponen Lab dikelompokkan seperti `CreateByExaminationsAsync` (per disiplin). Pesanan baru: `CoveringMcuOrderId = McuOrder.Id`, `InstructingDoctorId = McuDoctorId`, `InstructionVerificationStatus = NotRequired` |
| Rad | Satu pesanan per komponen: `ProcedureId`, `ModalityId = RadModalityId`, `CoveringMcuOrderId`, `InstructingDoctorId` |
| Penahanan tagihan | `LabSpecimenService` dan `RadStudyService` tidak menerbitkan fakta tagih bila `CoveringMcuOrderId` terisi (`MCU-DEC-022`) |
| Komponen di luar paket | Pesanan **tanpa** `CoveringMcuOrderId` → ditagih biasa (`MCU-DEC-036`) |
| Ganti dokter MCU | Pesanan terbuka milik order diperbarui `InstructingDoctorId`-nya dalam transaksi yang sama (`MCU-DEC-040`) |
| Rekonsiliasi | Pemeriksaan berkala (dapat berupa kueri admin): komponen ber-`LabOrderId` yang pesanannya tidak ber-`CoveringMcuOrderId` sama (atau sebaliknya) dilaporkan sebagai selisih |
| Nilai kritis | Diterima dokter MCU lewat `InstructingDoctorId`; mekanisme pemberitahuan tetap milik Lab (`LAB-COORD-017`) |

### `INT-MCU-09` — Refund

| Butir | Isi |
| --- | --- |
| Batal komponen | `SelectedBillingItemIds = [PackageInvoiceItemId]`, `RequestedAmount = RefundAmountSnapshot`, `Reason` = alasan dokter |
| Batal penuh | `RequestedAmount` = nilai baris paket |
| Peserta perusahaan | Sesuai aturan Billing untuk tagihan berpenjamin — `MCU-COORD-003` |
| Belum dibayar | Tidak ada refund; perorangan belum lunas tidak mungkin punya komponen dimulai (`VAL-MCU-34`) |
| Persetujuan refund | Tetap di Billing (`POST refunds/{id}/approve`) |
| Terbukti **tidak** didukung (2026-10-21) | Nominal sebagian (`MCU-UNK-04`) — `MCU-EVD-002` 2.2; diperluas Billing sesuai `MCU-DEC-043` |
| Terbukti didukung (2026-10-21) | Batal satu pemeriksaan Lab dalam pesanan berisi beberapa pemeriksaan (`ARC-TECH-01`) — `MCU-EVD-002` 2.1 |

### `INT-MCU-10` — Sumber jasa medis

| Butir | Isi |
| --- | --- |
| Isi | Per komponen selesai ber-`MedicalFeeAmountSnapshot > 0`: nomor order, komponen, dokter pelaksana (Lab: `ExaminerDoctorId`; Rad: `ExaminingDoctorId`; Pemeriksaan MCU: pengisi/dokter MCU), nilai, waktu |
| Komponen batal | Tidak pernah muncul |
| Status | **Draf** — bentuk akhir (tarik oleh Medical Fee vs dorong oleh MCU) diputuskan bersama pemilik Medical Fee |

## 3. Kegagalan dan percobaan ulang

| Kejadian | Perilaku |
| --- | --- |
| Check-in gagal di tengah | Transaksi dibatalkan; tidak ada kunjungan/baris yatim; petugas mengulang |
| *Mulai pemeriksaan* gagal di Lab | Seluruh transaksi dibatalkan; tidak ada pesanan setengah jadi |
| Status Lab/Rad tidak sinkron | Tombol *refresh* membaca ulang; tidak ada penyimpanan status ganda selain `ComponentStatus` turunan |
| Tutup hari gagal sebagian | `McuNoShowHostedService` memproses per order; order yang gagal dicoba pada siklus berikutnya |
