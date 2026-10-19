# Roadmap Backend — Perbaikan Admisi Bayi 022

Kontrak terkunci: [`RWI-BAYI-022-2.0.0`](../contracts/newborn-admission-repair-022.md), disetujui
pengguna 16 Oktober 2026. QBE preflight wajib diulang pada setiap task. Automated test backend
`NOT APPLICABLE` sesuai `rules/backend/TEST_POLICY.md`; bukti minimum adalah review scope,
verifikasi kontrak/proses bisnis, dan `dotnet build` aplikasi.

## Grafik Urutan Dependency

```text
BE-RWI-245 ─> BE-RWI-244 ─┐
                          ├─> BE-RWI-247
BE-RWI-246 ───────────────┘
```

| Gelombang | Boleh mulai setelah | Task |
| ---: | --- | --- |
| 1 | Kontrak `RWI-BAYI-022-2.0.0` | `BE-RWI-245`, `BE-RWI-246` |
| 2 | `BE-RWI-245` | `BE-RWI-244` |
| 3 | `BE-RWI-244`, `BE-RWI-245`, `BE-RWI-246` | `BE-RWI-247` |

### `BE-RWI-244` — Calon ibu berbasis kasus persalinan

| Field | Nilai |
| --- | --- |
| Outcome | Endpoint berhalaman hanya menampilkan kasus obstetri `InProgress`/`Completed` tanpa syarat ruang operasi |
| Trace | `PKJ-AB-02`; kontrak bagian 1.1 dan 3 |
| Reuse | `OprCase`, `RegPatientEncounterGuarantor`, `InpEpisode`, `PagedResult<T>` |
| Dependency | `BE-RWI-245` — field kasus dan urutan dipakai untuk count/preview |
| Klasifikasi | `HEAVY` — query lintas empat aggregate, tanpa mutasi schema |
| Status | ✅ Selesai source; runtime menunggu penerapan migration |

Acceptance criteria:

1. Kasus `Obstetric` berstatus `InProgress` dan `Completed` muncul; status lain dan bedah umum tidak.
2. Ibu tetap muncul ketika ruang aktifnya bukan ruang operasi atau tidak mempunyai episode aktif.
3. Response memuat identitas, kasus, sumber penjamin, lokasi saat ini, jumlah bayi, dan preview urutan.
4. Search, tanggal, pagination, cancellation token, permission, dan envelope API mengikuti pola existing.

Definition of Done: source dan laporan tersedia, kontrak endpoint diverifikasi, `dotnet build` dijalankan, roadmap serta traceability diperbarui.

### `BE-RWI-245` — Registrasi pasien bayi atomik dan idempotent

| Field | Nilai |
| --- | --- |
| Outcome | Endpoint khusus membuat bayi bernama sementara dan urutan kelahiran backend per kasus |
| Trace | `PKJ-AB-03`; kontrak bagian 1.2–1.5, 3, 4 |
| Reuse | `MstPatient`, alokasi MR existing melalui provider number-series, `OprCase` |
| Dependency | Kontrak `RWI-BAYI-022-2.0.0` |
| Klasifikasi | `HEAVY` — entity operasional `Pat`, transaksi, index unik, migration |
| Status | ✅ Selesai source dan migration; database belum diterapkan |

Acceptance criteria:

1. Request tidak menerima urutan; bayi pertama mendapat 1 dan bayi berikutnya pada kasus sama bertambah satu.
2. Kasus berbeda memulai urutan 1 walau ibunya sama.
3. Alokasi memakai lock database dan pencacah tersimpan, bukan `Count+1`/`Max+1`.
4. Kunci idempotency sama + payload sama mengembalikan pasien sama; payload berbeda ditolak `409`.
5. Relasi ibu/kasus diverifikasi, nama `By. Ny. <ibu>` dan nomor rekam medis sendiri tersimpan.
6. Migration, Designer, dan snapshot tersedia tetapi database tidak diterapkan.

Definition of Done: source dan laporan tersedia, skenario concurrency ditelusuri pada source, migration direview, `dotnet build` dijalankan, register diperbarui.

### `BE-RWI-246` — Snapshot penjamin bayi mengikuti ibu

| Field | Nilai |
| --- | --- |
| Outcome | Encounter bayi mewarisi snapshot tunai/asuransi/perusahaan beserta provenance sumber |
| Trace | `PKJ-AB-04`; kontrak bagian 1.6, 3, 4 |
| Reuse | `RegPatientEncounterGuarantor`, `PatientEncounterController`, payment source existing |
| Dependency | Kontrak `RWI-BAYI-022-2.0.0` |
| Klasifikasi | `HEAVY` — perubahan kontrak registrasi dan schema provenance |
| Status | ✅ Selesai source dan migration; runtime menunggu penerapan migration |

Acceptance criteria:

1. Hanya pasien bayi dengan ibu/kasus yang cocok dapat memakai sumber encounter ibu.
2. Snapshot payer disalin dan `InheritedFromPaymentSourceId` dapat diaudit.
3. FK kartu milik ibu tidak dipasang pada pasien bayi.
4. Sumber yang berubah sesudah kandidat dimuat ditolak `409` tanpa encounter parsial.
5. Jalur encounter non-bayi tetap kompatibel.

Definition of Done: source dan laporan tersedia, tiga jenis penjamin ditelusuri, migration direview, `dotnet build` dijalankan, register diperbarui.

### `BE-RWI-247` — Konteks episode, bed, dan handoff Billing

| Field | Nilai |
| --- | --- |
| Outcome | Episode menyimpan kasus persalinan tanpa input finansial dan memakai kelayakan unit/bed yang sama |
| Trace | `PKJ-AB-05`, `PKJ-AB-09`; kontrak bagian 1.7–1.8, 3, 4 |
| Reuse | `InpNewbornAdmissionRules`, `ServiceUnitRegistrationRules`, event Billing existing |
| Dependency | `BE-RWI-244`, `BE-RWI-245`, `BE-RWI-246` |
| Klasifikasi | `MEDIUM` — adapter kompatibilitas pada episode/bed |
| Status | ✅ Selesai source; live integration menunggu penerapan migration |

Acceptance criteria:

1. `DeliveryCaseId` episode cocok dengan pasien bayi dan kasus obstetri.
2. Episode baru sah tanpa `FamilyBillingPreference` dan `PlannedDepositAmount`; pembacaan data lama tetap bekerja.
3. Event layanan tetap membawa episode/encounter bayi sendiri tanpa mutasi saldo deposit.
4. Bed non-bayi atau unit tak layak registrasi tidak dapat dipakai dan respons dapat diterjemahkan ramah oleh frontend.

Definition of Done: source dan laporan tersedia, jalur episode/bed/event ditelusuri, `dotnet build` dijalankan, register diperbarui.

## Register Status

| Task | Status | Laporan |
| --- | :---: | --- |
| `BE-RWI-244` | ✅ Selesai source | [BE-RWI-244.md](../task/report/backend/BE-RWI-244.md) |
| `BE-RWI-245` | ✅ Selesai source + migration | [BE-RWI-245.md](../task/report/backend/BE-RWI-245.md) |
| `BE-RWI-246` | ✅ Selesai source + migration | [BE-RWI-246.md](../task/report/backend/BE-RWI-246.md) |
| `BE-RWI-247` | ✅ Selesai source | [BE-RWI-247.md](../task/report/backend/BE-RWI-247.md) |

